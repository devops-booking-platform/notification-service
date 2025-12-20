using Microsoft.AspNetCore.Authentication.JwtBearer;
using NotificationService.Common.Events;
using NotificationService.Configuration;
using NotificationService.Domain.Mappings;
using NotificationService.IntegrationEvents.Handlers;
using NotificationService.Repositories;
using NotificationService.Repositories.Interfaces;
using NotificationService.Services;
using NotificationService.Services.Interfaces;

namespace NotificationService.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccommodationServiceDependencies(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<INotificationDisabledService, NotificationDisabledService>();
        services.AddScoped<INotificationService, Services.NotificationService>();
        services.AddScoped<IIntegrationEventDispatcher, IntegrationEventDispatcher>();

        services.AddScoped<IIntegrationEventHandler<HostRatedIntegrationEvent>, HostRatedIntegrationIntegrationEventHandler>();
        services.AddScoped<IIntegrationEventHandler<AccommodationRatedIntegrationEvent>, AccommodationRatedIntegrationIntegrationEventHandler>();

        services.AddScoped<IRoutedIntegrationEventHandler, RoutedHandler<HostRatedIntegrationEvent>>();

        services.AddHostedService<IntegrationEventsSubscriber>();
        services.AddAutoMapper(cfg => cfg.AddProfile<NotificationMappingProfile>());

        return services;
    }

    public static IServiceCollection AddSwaggerGenWithAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme,
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    BearerFormat = "JWT"
                });

            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        return services;
    }
}