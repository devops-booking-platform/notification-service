using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Common.Events;
using NotificationService.Common.Hubs;
using NotificationService.Domain.DTOs;
using NotificationService.Domain.Entities;
using NotificationService.Repositories.Interfaces;

namespace NotificationService.IntegrationEvents.Handlers;

public sealed class ReservationCanceledIntegrationEventHandler(
    ILogger<ReservationCanceledIntegrationEventHandler> logger,
    IRepository<Notification> notificationRepository,
    IRepository<NotificationDisabled> notificationDisabledRepository,
    IHubContext<NotificationHub> hubContext,
    IUnitOfWork unitOfWork)
    : IIntegrationEventHandler<ReservationCanceledIntegrationEvent>
{
    public async Task Handle(ReservationCanceledIntegrationEvent @event, CancellationToken ct)
    {
        var isNotificationDisabled = await notificationDisabledRepository
            .Query()
            .Where(x => x.UserId == @event.HostId && x.NotificationType == NotificationType.ReservationCanceled)
            .AnyAsync(ct);
        if (isNotificationDisabled)
        {
            return;
        }
        logger.LogInformation("Handling Reservation canceled for HostId={UserId}, ReservationId={ReservationId}",
            @event.HostId, @event.ReservationId);
        var message =
            $"The Reservation for {@event.AccommodationName} from {@event.StartDate} to {@event.EndDate} was cancelled by {@event.GuestUsername}.";
        var notification = Notification.Create(@event.HostId, NotificationType.ReservationCanceled, message);

        await notificationRepository.AddAsync(notification);

        await unitOfWork.SaveChangesAsync(ct);

        await hubContext
            .Clients
            .User(@event.HostId.ToString())
            .SendAsync("ReceiveMessage",
                new NotificationDto
                (
                    notification.Id,
                    NotificationType.ReservationCanceled,
                    message,
                    notification.CreatedOn), ct);
    }
}