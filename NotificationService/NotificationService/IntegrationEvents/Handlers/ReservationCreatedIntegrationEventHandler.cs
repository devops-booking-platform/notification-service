using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Common.Events;
using NotificationService.Common.Hubs;
using NotificationService.Domain.DTOs;
using NotificationService.Domain.Entities;
using NotificationService.Repositories.Interfaces;

namespace NotificationService.IntegrationEvents.Handlers;

public sealed class ReservationCreatedIntegrationEventHandler(
    ILogger<ReservationCreatedIntegrationEventHandler> logger,
    IRepository<Notification> notificationRepository,
    IRepository<NotificationDisabled> notificationDisabledRepository,
    IHubContext<NotificationHub> hubContext,
    IUnitOfWork unitOfWork)
    : IIntegrationEventHandler<ReservationCreatedIntegrationEvent>
{
    public async Task Handle(ReservationCreatedIntegrationEvent @event, CancellationToken ct)
    {
        var isNotificationDisabled = await notificationDisabledRepository
            .Query()
            .Where(x => x.UserId == @event.HostId && x.NotificationType == NotificationType.ReservationCreated)
            .AnyAsync(ct);
        if (isNotificationDisabled)
        {
            return;
        }
        logger.LogInformation("Handling Reservation created for HostId={UserId}, ReservationId={ReservationId}",
            @event.HostId, @event.ReservationId);
        
        var message =
            $"You have a new reservation for {@event.AccommodationName} from {@event.StartDate} to {@event.EndDate}. The reservation was created by {@event.GuestUsername}.";
        var notification = Notification.Create(@event.HostId, NotificationType.ReservationCreated, message);

        await notificationRepository.AddAsync(notification);

        await unitOfWork.SaveChangesAsync(ct);

        await hubContext
            .Clients
            .User(@event.HostId.ToString())
            .SendAsync("ReceiveMessage",
                new NotificationDto
                (
                    notification.Id,
                    NotificationType.ReservationCreated,
                    message,
                    notification.CreatedOn), ct);
    }
}