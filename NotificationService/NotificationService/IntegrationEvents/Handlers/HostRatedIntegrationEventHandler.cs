using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Common.Events;
using NotificationService.Common.Hubs;
using NotificationService.Domain.Entities;
using NotificationService.Repositories.Interfaces;

namespace NotificationService.IntegrationEvents.Handlers;

public sealed class HostRatedIntegrationEventHandler(
    ILogger<HostRatedIntegrationEventHandler> logger,
    IRepository<Notification> notificationRepository,
    IRepository<NotificationDisabled> notificationDisabledRepository,
    IHubContext<NotificationHub> hubContext,
    IUnitOfWork unitOfWork)
    : IIntegrationEventHandler<HostRatedIntegrationEvent>
{
    public async Task Handle(HostRatedIntegrationEvent @event, CancellationToken ct)
    {
        var isNotificationDisabled = await notificationDisabledRepository
            .Query()
            .Where(x => x.UserId == @event.HostId && x.NotificationType == NotificationType.HostRated)
            .AnyAsync(ct);
        if (isNotificationDisabled)
        {
            return;
        }

        logger.LogInformation("Handling HostRatedIntegrationEvent for HostId={UserId}", @event.HostId);

        var message = $"{@event.GuestUsername} left you a review with rating {@event.Rating}";
        var notification = Notification.Create(@event.HostId, NotificationType.HostRated, message);

        await notificationRepository.AddAsync(notification);

        await unitOfWork.SaveChangesAsync(ct);

        await hubContext
            .Clients
            .User(@event.HostId.ToString())
            .SendAsync("ReceiveMessage",
                new
                {
                    notification.Id,
                    Type = NotificationType.HostRated,
                    Message = message,
                    notification.CreatedOn
                }, ct);
    }
}