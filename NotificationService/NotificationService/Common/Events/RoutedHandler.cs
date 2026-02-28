using System.Text.Json;

namespace NotificationService.Common.Events;

public sealed class RoutedHandler<TEvent>(IIntegrationEventHandler<TEvent> inner)
    : IRoutedIntegrationEventHandler
{
    public string RoutingKey => typeof(TEvent).Name;

    public async Task HandleJson(string json, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<TEvent>(json)
                  ?? throw new InvalidOperationException($"Failed to deserialize {typeof(TEvent).Name}");
        await inner.Handle(evt, ct);
    }
}