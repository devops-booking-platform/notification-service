using NotificationService.Domain.Entities;

namespace NotificationService.Repositories;

public static class NotificationQueryableExtensions
{
    public static IQueryable<Notification> QueryByRead(this IQueryable<Notification> query, bool? read) =>
        read.HasValue ? query.Where(n => n.Read == read) : query;

    public static IQueryable<Notification> QueryByNotificationType(this IQueryable<Notification> query,
        NotificationType? type) =>
        type.HasValue ? query.Where(n => n.NotificationType == type) : query;

    public static IQueryable<Notification> QueryByUserId(this IQueryable<Notification> query,
        Guid userId) => query.Where(n => n.UserId == userId);
}