using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Common.Events;
using NotificationService.Common.Hubs;
using NotificationService.Domain.DTOs;
using NotificationService.Domain.Entities;
using NotificationService.Repositories.Interfaces;

namespace NotificationService.IntegrationEvents.Handlers;

public sealed class ReservationRespondedIntegrationEventHandler(
    ILogger<ReservationRespondedIntegrationEventHandler> logger,
    IRepository<Notification> notificationRepository,
    IRepository<NotificationDisabled> notificationDisabledRepository,
    IHubContext<NotificationHub> hubContext,
    IUnitOfWork unitOfWork)
    : IIntegrationEventHandler<ReservationRespondedIntegrationEvent>
{
    public async Task Handle(ReservationRespondedIntegrationEvent @event, CancellationToken ct)
    {
        var isNotificationDisabled = await notificationDisabledRepository
            .Query()
            .Where(x => x.UserId == @event.GuestId && x.NotificationType == NotificationType.ReservationResponded)
            .AnyAsync(ct);
        if (isNotificationDisabled)
        {
            return;
        }

        logger.LogInformation(
            "Handling Reservation responded for GuestId={GuestId}, ReservationId={ReservationId}, IsApproved={IsApproved}",
            @event.GuestId, @event.ReservationId, @event.IsApproved);
        
        var message =
            $"The Reservation for {@event.AccommodationName} has been {(@event.IsApproved ? "Approved" : "Rejected")}.";
        var notification = Notification.Create(@event.GuestId, NotificationType.ReservationResponded, message);

        await notificationRepository.AddAsync(notification);

        await unitOfWork.SaveChangesAsync(ct);

        await hubContext
            .Clients
            .User(@event.GuestId.ToString())
            .SendAsync("ReceiveMessage",
                new NotificationDto
                (
                    notification.Id,
                    NotificationType.ReservationResponded,
                    message,
                    notification.CreatedOn), ct);
    }
}