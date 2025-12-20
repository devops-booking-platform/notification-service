using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using NotificationService.Common.Events;

namespace NotificationService.Configuration;

public sealed class IntegrationEventsSubscriber : BackgroundService
{
    private const string QueueName = "notification.integration";

    private readonly RabbitMqSettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IntegrationEventsSubscriber> _logger;

    private IConnection? _connection;
    private IChannel? _channel;
    private CancellationToken _stoppingToken;

    public IntegrationEventsSubscriber(
    IOptions<RabbitMqSettings> options,
    IServiceProvider serviceProvider,
    ILogger<IntegrationEventsSubscriber> logger)
    {
        _settings = options.Value;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _stoppingToken = stoppingToken;

        var factory = new ConnectionFactory
        {
            HostName = _settings.Host,
            UserName = _settings.User,
            Password = _settings.Pass,
            VirtualHost = _settings.VirtualHost,
            AutomaticRecoveryEnabled = true
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(
            exchange: _settings.Exchange,
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.BasicQosAsync(0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(QueueName, _settings.Exchange, nameof(HostRatedIntegrationEvent), cancellationToken: stoppingToken);
        await _channel.QueueBindAsync(QueueName, _settings.Exchange, nameof(AccommodationRatedIntegrationEvent), cancellationToken: stoppingToken);
        await _channel.QueueBindAsync(QueueName, _settings.Exchange, nameof(ReservationCreatedIntegrationEvent), cancellationToken: stoppingToken);
        await _channel.QueueBindAsync(QueueName, _settings.Exchange, nameof(ReservationCanceledIntegrationEvent), cancellationToken: stoppingToken);
        await _channel.QueueBindAsync(QueueName, _settings.Exchange, nameof(ReservationRespondedIntegrationEvent), cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessage;

        await _channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("RabbitMQ subscriber started. Exchange={Exchange} Queue={Queue}",
            _settings.Exchange, QueueName);
    }

    private async Task OnMessage(object sender, BasicDeliverEventArgs ea)
    {
        if (_channel is null) return;

        try
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());

            using var scope = _serviceProvider.CreateScope();
            var dispatcher = scope.ServiceProvider.GetRequiredService<IIntegrationEventDispatcher>();

            await dispatcher.Dispatch(ea.RoutingKey, json, _stoppingToken);

            await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle message. RoutingKey={RoutingKey} DeliveryTag={Tag}",
                ea.RoutingKey, ea.DeliveryTag);

            await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
        }
    }


    public override void Dispose()
    {
        try { _channel?.Dispose(); } catch { }
        try { _connection?.Dispose(); } catch { }
        base.Dispose();
    }
}
