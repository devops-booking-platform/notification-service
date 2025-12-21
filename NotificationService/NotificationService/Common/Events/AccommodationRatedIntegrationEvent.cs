namespace NotificationService.Common.Events;

public record AccommodationRatedIntegrationEvent(Guid HostId, Guid AccommodationId, string GuestUsername, int Rating)
    : IIntegrationEvent;