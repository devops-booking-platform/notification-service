namespace NotificationService.Common.Events;

public interface IIntegrationEventDispatcher
{
    Task Dispatch(string routingKey, string json, CancellationToken ct);
}

public sealed class IntegrationEventDispatcher : IIntegrationEventDispatcher
{
    private readonly IReadOnlyDictionary<string, IRoutedIntegrationEventHandler> _map;

    public IntegrationEventDispatcher(IEnumerable<IRoutedIntegrationEventHandler> handlers)
        => _map = handlers.ToDictionary(h => h.RoutingKey, h => h);

    public Task Dispatch(string routingKey, string json, CancellationToken ct)
        => _map.TryGetValue(routingKey, out var handler)
            ? handler.HandleJson(json, ct)
            : throw new InvalidOperationException($"No handler registered for routingKey={routingKey}");
}