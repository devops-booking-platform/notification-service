namespace NotificationService.Common.Events;

public record HostRatedIntegrationEvent(Guid HostId, string GuestUsername, int Rating) : IIntegrationEvent;