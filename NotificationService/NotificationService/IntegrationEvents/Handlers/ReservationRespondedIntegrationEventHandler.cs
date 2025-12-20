using NotificationService.Common.Events;

namespace NotificationService.IntegrationEvents.Handlers;

public sealed class ReservationRespondedIntegrationEventHandlerHandler(
    ILogger<ReservationRespondedIntegrationEventHandlerHandler> logger)
    : IIntegrationEventHandler<ReservationRespondedIntegrationEvent>
{
    public async Task Handle(ReservationRespondedIntegrationEvent @event, CancellationToken ct)
    {
        logger.LogInformation(
            "Handling Reservation responded for GuestId={GuestId}, ReservationId={ReservationId}, IsApproved={IsApproved}",
            @event.GuestId, @event.ReservationId, @event.IsApproved);
        await Task.CompletedTask;
    }
}