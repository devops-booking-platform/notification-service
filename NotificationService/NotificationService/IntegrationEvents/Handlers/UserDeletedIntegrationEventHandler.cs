using NotificationService.Common.Events;

namespace NotificationService.IntegrationEvents.Handlers;

public sealed class UserDeletedIntegrationEventHandler(ILogger<UserDeletedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<UserDeletedIntegrationEvent>
{
    public async Task Handle(UserDeletedIntegrationEvent @event, CancellationToken ct)
    {
        logger.LogInformation("Handling UserDeletedIntegrationEvent for UserId={UserId}, Role={Role}",
            @event.UserId, @event.Role);
        await Task.CompletedTask;
    }
}