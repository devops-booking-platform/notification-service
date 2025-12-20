using AutoMapper;
using NotificationService.Common.Exceptions;
using NotificationService.Domain.DTOs;
using NotificationService.Domain.Entities;
using NotificationService.Repositories;
using NotificationService.Repositories.Interfaces;
using NotificationService.Services.Interfaces;

namespace NotificationService.Services;

public class NotificationService(
    IRepository<Notification> notificationsRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IMapper mapper) : INotificationService
{
    public async Task<PagedResult<GetNotificationResponse>> GetNotifications(GetNotificationRequest request)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("You don't have access to this action.");
        }

        return await notificationsRepository
            .Query()
            .QueryByNotificationType(request.NotificationType)
            .QueryByRead(request.Read)
            .QueryByUserId(userId.Value)
            .OrderByDescending(n => n.CreatedOn)
            .ToPagedAsync<Notification, GetNotificationResponse>(
                request.Page,
                request.PageSize,
                mapper.ConfigurationProvider);
    }

    public async Task<GetNotificationResponse> GetNotification(Guid id)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("You don't have access to this action.");
        }

        var notification = await notificationsRepository
            .GetByIdAsync(id) ?? throw new NotFoundException("Notification not found");
        return mapper.Map<GetNotificationResponse>(notification);
    }

    public async Task MarkAsRead(Guid id)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("You don't have access to this action.");
        }
        var notification = await notificationsRepository
            .GetByIdAsync(id) ?? throw new NotFoundException("Notification not found");
        
        notification.MarkRead();
        await unitOfWork.SaveChangesAsync();
    }
}