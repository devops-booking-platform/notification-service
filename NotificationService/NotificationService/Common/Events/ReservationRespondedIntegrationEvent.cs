namespace NotificationService.Common.Events;

public record ReservationRespondedIntegrationEvent(
    Guid GuestId,
    Guid ReservationId,
    string AccommodationName,
    bool IsApproved)
    : IIntegrationEvent;