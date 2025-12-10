namespace NotificationService.Common.Events;

public interface IRoutedIntegrationEventHandler
{
    string RoutingKey { get; }
    Task HandleJson(string json, CancellationToken ct);
}