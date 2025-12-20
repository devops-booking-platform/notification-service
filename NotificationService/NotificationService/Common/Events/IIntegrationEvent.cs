namespace NotificationService.Common.Events;

public interface IIntegrationEvent
{
}

public record HostRatedIntegrationEvent(Guid HostId) : IIntegrationEvent;
public record AccommodationRatedIntegrationEvent(Guid HostId, Guid AccommodationId) : IIntegrationEvent;

public record ReservationCreatedIntegrationEvent(Guid HostId, Guid ReservationId) : IIntegrationEvent;
public record ReservationCanceledIntegrationEvent(Guid HostId, Guid ReservationId) : IIntegrationEvent;
public record ReservationRespondedIntegrationEvent(Guid GuestId, Guid ReservationId, bool IsApproved) : IIntegrationEvent;
