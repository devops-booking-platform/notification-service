using NotificationService.Common.Events;

namespace NotificationService.IntegrationEvents.Handlers;

public sealed class ReservationCanceledIntegrationEventHandler(
    ILogger<ReservationCanceledIntegrationEventHandler> logger)
    : IIntegrationEventHandler<ReservationCanceledIntegrationEvent>
{
    public async Task Handle(ReservationCanceledIntegrationEvent @event, CancellationToken ct)
    {
        logger.LogInformation("Handling Reservation canceled for HostId={UserId}, ReservationId={ReservationId}",
            @event.HostId, @event.ReservationId);
        await Task.CompletedTask;
    }
}