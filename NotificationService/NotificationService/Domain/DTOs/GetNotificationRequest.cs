using NotificationService.Domain.Entities;

namespace NotificationService.Domain.DTOs;

public class GetNotificationRequest : PagedRequest
{
    public bool? Read { get; set; }
    public NotificationType? NotificationType { get; set; }
}