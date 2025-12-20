using NotificationService.Domain.DTOs;

namespace NotificationService.Services.Interfaces;

public interface INotificationDisabledService
{
    public Task DisableNotification(EnableDisableNotificationRequest request);
    public Task EnableNotification(EnableDisableNotificationRequest request);
    public Task<ICollection<GetDisabledNotificationsResponse>> GetDisabledNotifications();
}