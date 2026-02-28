namespace NotificationService.Domain.Entities;

public class NotificationDisabled : EntityWithGuidId
{
    public Guid UserId { get; set; }
    public NotificationType NotificationType { get; set; }

    public static NotificationDisabled Create(Guid userId, NotificationType notificationType)
        => new()
        {
            UserId = userId,
            NotificationType = notificationType
        };
}