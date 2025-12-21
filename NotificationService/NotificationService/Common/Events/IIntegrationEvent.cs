namespace NotificationService.Common.Events;

public interface IIntegrationEvent
{
}

public record HostRatedIntegrationEvent(Guid HostId, string GuestUsername, int Rating) : IIntegrationEvent;

public record AccommodationRatedIntegrationEvent(Guid HostId, Guid AccommodationId, string GuestUsername, int Rating)
    : IIntegrationEvent;

public record ReservationCreatedIntegrationEvent(
    Guid HostId,
    Guid ReservationId,
    string AccommodationName,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string GuestUsername) : IIntegrationEvent;

public record ReservationCanceledIntegrationEvent( Guid HostId,
    Guid ReservationId,
    string AccommodationName,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string GuestUsername) : IIntegrationEvent;

public record ReservationRespondedIntegrationEvent(Guid GuestId, Guid ReservationId, string AccommodationName, bool IsApproved)
    : IIntegrationEvent;