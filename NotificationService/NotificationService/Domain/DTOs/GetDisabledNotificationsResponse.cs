using NotificationService.Domain.Entities;

namespace NotificationService.Domain.DTOs;

public class GetDisabledNotificationsResponse
{
    public Guid Id { get; set; }
    public NotificationType NotificationType { get; set; }
}