using Insight.Invoicing.Application.IntegrationEvents;
using Insight.Invoicing.Application.Services;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Insight.Invoicing.Infrastructure.Services;

public class MassTransitEventBus : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<MassTransitEventBus> _logger;

    public MassTransitEventBus(IPublishEndpoint publishEndpoint, ILogger<MassTransitEventBus> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : class, IIntegrationEvent
    {
        try
        {
            _logger.LogInformation("Publishing integration event {EventType} with ID {EventId}",
                integrationEvent.EventType, integrationEvent.Id);

            await _publishEndpoint.Publish(integrationEvent, cancellationToken);

            _logger.LogInformation("Successfully published integration event {EventType} with ID {EventId}",
                integrationEvent.EventType, integrationEvent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish integration event {EventType} with ID {EventId}",
                integrationEvent.EventType, integrationEvent.Id);
            throw;
        }
    }

    public async Task PublishAsync(IEnumerable<IIntegrationEvent> integrationEvents, CancellationToken cancellationToken = default)
    {
        var events = integrationEvents.ToList();
        if (!events.Any())
        {
            return;
        }

        try
        {
            _logger.LogInformation("Publishing {Count} integration events", events.Count);

            var tasks = events.Select(evt => _publishEndpoint.Publish(evt, evt.GetType(), cancellationToken));
            await Task.WhenAll(tasks);

            _logger.LogInformation("Successfully published {Count} integration events", events.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish {Count} integration events", events.Count);
            throw;
        }
    }
}


