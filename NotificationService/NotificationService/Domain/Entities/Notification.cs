namespace NotificationService.Domain.Entities;

public class Notification : EntityWithGuidId
{
    public const int MessageMaxlength = 2056;
    public Guid UserId { get; set; }
    public NotificationType NotificationType { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool Read { get; set; }


    public static Notification Create(Guid userId, NotificationType notificationType, string message)
        => new()
        {
            UserId = userId,
            NotificationType = notificationType,
            Message = message,
            Read = false,
            CreatedOn = DateTimeOffset.UtcNow
        };
    
    public void MarkRead() => Read = true;
}