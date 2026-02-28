namespace NotificationService.Common.Events;

public record AccommodationRatedIntegrationEvent(
    Guid HostId,
    Guid AccommodationId,
    string GuestUsername,
    string AccommodationName,
    int Rating)
    : IIntegrationEvent;