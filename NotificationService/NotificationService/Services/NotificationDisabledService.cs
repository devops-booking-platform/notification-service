using Microsoft.EntityFrameworkCore;
using NotificationService.Common.Exceptions;
using NotificationService.Domain.DTOs;
using NotificationService.Domain.Entities;
using NotificationService.Repositories.Interfaces;
using NotificationService.Services.Interfaces;

namespace NotificationService.Services;

public class NotificationDisabledService(
    IRepository<NotificationDisabled> notificationDisabledRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : INotificationDisabledService
{
    public async Task DisableNotification(EnableDisableNotificationRequest request)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("You don't have access to this action.");
        }

        var newDisabledNotification = NotificationDisabled.Create(userId.Value, request.NotificationType);
        await notificationDisabledRepository.AddAsync(newDisabledNotification);
        
        await unitOfWork.SaveChangesAsync();
    }

    public async Task EnableNotification(EnableDisableNotificationRequest request)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("You don't have access to this action.");
        }

        if (!request.Id.HasValue)
        {
            throw new UnauthorizedAccessException("You don't have access to this action.");
        }

        var disabledNotification = await notificationDisabledRepository
            .Query()
            .Where(x => x.Id == request.Id)
            .SingleOrDefaultAsync();

        if (disabledNotification == null)
        {
            throw new NotFoundException("Notification is not disabled.");
        }
        
        notificationDisabledRepository.Remove(disabledNotification);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task<ICollection<GetDisabledNotificationsResponse>> GetDisabledNotifications()
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("You don't have access to this action.");
        }

        return await notificationDisabledRepository
            .Query()
            .Where(x => x.UserId == userId)
            .Select(x => new GetDisabledNotificationsResponse
            {
                Id = x.Id,
                NotificationType = x.NotificationType
            })
            .ToListAsync();
    }
}