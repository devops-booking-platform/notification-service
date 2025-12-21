using NotificationService.Domain.DTOs;

namespace NotificationService.Services.Interfaces;

public interface INotificationService
{
    Task<PagedResult<GetNotificationResponse>> GetNotifications(GetNotificationRequest request);
    Task<PagedResult<GetNotificationResponse>> GetUnreadNotifications();
    Task<GetNotificationResponse> GetNotification(Guid id);
    Task MarkAsRead(Guid id);
}