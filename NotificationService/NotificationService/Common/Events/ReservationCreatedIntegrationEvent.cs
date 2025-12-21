namespace NotificationService.Common.Events;

public record ReservationCreatedIntegrationEvent(
    Guid HostId,
    Guid ReservationId,
    string AccommodationName,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string GuestUsername) : IIntegrationEvent;