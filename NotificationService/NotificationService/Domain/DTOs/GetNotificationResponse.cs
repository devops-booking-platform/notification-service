using NotificationService.Domain.Entities;

namespace NotificationService.Domain.DTOs;

public class GetNotificationResponse
{
    public Guid Id { get; set; }
    public NotificationType NotificationType { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool Read { get; set; }
}