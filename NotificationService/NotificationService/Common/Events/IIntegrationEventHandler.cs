namespace NotificationService.Common.Events;

public interface IIntegrationEventHandler<in T>
{
    Task Handle(T @event, CancellationToken ct);
}