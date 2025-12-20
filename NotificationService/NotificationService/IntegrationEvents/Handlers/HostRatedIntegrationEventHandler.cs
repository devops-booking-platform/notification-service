using NotificationService.Common.Events;

namespace NotificationService.IntegrationEvents.Handlers;

public sealed class HostRatedIntegrationEventHandler(
    ILogger<HostRatedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<HostRatedIntegrationEvent>
{
    public async Task Handle(HostRatedIntegrationEvent @event, CancellationToken ct)
    {
        logger.LogInformation("Handling HostRatedIntegrationEvent for HostId={UserId}", @event.HostId);
        await Task.CompletedTask;
    }
}