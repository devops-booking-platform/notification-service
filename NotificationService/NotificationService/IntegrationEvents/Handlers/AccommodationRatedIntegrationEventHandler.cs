using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Common.Events;
using NotificationService.Common.Hubs;
using NotificationService.Domain.Entities;
using NotificationService.Repositories.Interfaces;

namespace NotificationService.IntegrationEvents.Handlers;

public sealed class AccommodationRatedIntegrationEventHandler(
    ILogger<AccommodationRatedIntegrationEventHandler> logger,
    IRepository<Notification> notificationRepository,
    IRepository<NotificationDisabled> notificationDisabledRepository,
    IHubContext<NotificationHub> hubContext,
    IUnitOfWork unitOfWork)
    : IIntegrationEventHandler<AccommodationRatedIntegrationEvent>
{
    public async Task Handle(AccommodationRatedIntegrationEvent @event, CancellationToken ct)
    {
        var isNotificationDisabled = await notificationDisabledRepository
            .Query()
            .Where(x => x.UserId == @event.HostId && x.NotificationType == NotificationType.AccommodationRated)
            .AnyAsync(ct);
        if (isNotificationDisabled)
        {
            return;
        }

        logger.LogInformation(
            "Handling AccommodationRatedIntegrationEvent for HostId={UserId} and AccommodationId={AccommodationId}",
            @event.HostId, @event.AccommodationId);

        var message =
            $"{@event.GuestUsername} left a review for {@event.AccommodationName} with rating {@event.Rating}";
        var notification = Notification.Create(@event.HostId, NotificationType.AccommodationRated, message);

        await notificationRepository.AddAsync(notification);

        await unitOfWork.SaveChangesAsync(ct);

        await hubContext
            .Clients
            .User(@event.HostId.ToString())
            .SendAsync("ReceiveMessage",
                new
                {
                    notification.Id,
                    Type = NotificationType.AccommodationRated,
                    Message = message,
                    notification.CreatedOn
                }, ct);
    }
}