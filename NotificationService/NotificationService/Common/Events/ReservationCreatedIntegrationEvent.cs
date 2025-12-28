namespace NotificationService.Common.Events;

public record ReservationCreatedIntegrationEvent(
    Guid HostId,
    Guid ReservationId,
    string AccommodationName,
    DateOnly StartDate,
    DateOnly EndDate,
    string GuestUsername) : IIntegrationEvent;