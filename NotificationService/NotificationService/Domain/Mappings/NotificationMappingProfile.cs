using AutoMapper;
using NotificationService.Domain.DTOs;
using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Mappings;

public class NotificationMappingProfile : Profile
{
    public NotificationMappingProfile()
    {
        CreateMap<Notification, GetNotificationResponse>();
    }
}