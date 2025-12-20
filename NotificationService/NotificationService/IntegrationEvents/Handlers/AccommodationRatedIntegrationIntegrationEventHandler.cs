using NotificationService.Common.Events;

namespace NotificationService.IntegrationEvents.Handlers;

public sealed class AccommodationRatedIntegrationIntegrationEventHandler(
    ILogger<AccommodationRatedIntegrationIntegrationEventHandler> logger)
    : IIntegrationEventHandler<AccommodationRatedIntegrationEvent>
{
    public async Task Handle(AccommodationRatedIntegrationEvent @event, CancellationToken ct)
    {
        logger.LogInformation(
            "Handling AccommodationRatedIntegrationEvent for HostId={UserId} and AccommodationId={AccommodationId}",
            @event.HostId, @event.AccommodationId);
        await Task.CompletedTask;
    }
}