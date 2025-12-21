using NotificationService.Domain.Entities;

namespace NotificationService.Domain.DTOs;

public sealed record NotificationDto(
    Guid Id,
    NotificationType Type,
    string Message,
    DateTimeOffset CreatedOn);