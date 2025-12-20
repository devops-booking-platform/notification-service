using NotificationService.Common.Events;

namespace NotificationService.IntegrationEvents.Handlers;

public sealed class ReservationCreatedIntegrationEventHandler(ILogger<ReservationCreatedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<ReservationCreatedIntegrationEvent>
{
    public async Task Handle(ReservationCreatedIntegrationEvent @event, CancellationToken ct)
    {
        logger.LogInformation("Handling Reservation created for HostId={UserId}, ReservationId={ReservationId}",
            @event.HostId, @event.ReservationId);
        await Task.CompletedTask;
    }
}