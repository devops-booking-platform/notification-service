using NotificationService.Common.Events;

namespace NotificationService.IntegrationEvents.Handlers;

public sealed class HostRatedIntegrationIntegrationEventHandler(
    ILogger<HostRatedIntegrationIntegrationEventHandler> logger)
    : IIntegrationEventHandler<HostRatedIntegrationEvent>
{
    public async Task Handle(HostRatedIntegrationEvent @event, CancellationToken ct)
    {
        logger.LogInformation("Handling HostRatedIntegrationEvent for HostId={UserId}", @event.HostId);
        await Task.CompletedTask;
    }
}