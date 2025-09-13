using Insight.Invoicing.Application.IntegrationEvents;
using Insight.Invoicing.Application.Services;
using Insight.Invoicing.Domain.Entities;
using Insight.Invoicing.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Insight.Invoicing.Infrastructure.Services;

public class OutboxService : IOutboxService
{
    private readonly IBaseRepository<OutboxEvent> _outboxRepository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<OutboxService> _logger;

    public OutboxService(
        IBaseRepository<OutboxEvent> outboxRepository,
        IEventBus eventBus,
        ILogger<OutboxService> logger)
    {
        _outboxRepository = outboxRepository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task AddEventAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : class, IIntegrationEvent
    {
        try
        {
            var eventData = JsonSerializer.Serialize(integrationEvent);
            var outboxEvent = new OutboxEvent(
                integrationEvent.Id,
                integrationEvent.EventType,
                eventData);

            await _outboxRepository.AddAsync(outboxEvent, cancellationToken);

            _logger.LogInformation("Added integration event {EventType} with ID {EventId} to outbox",
                integrationEvent.EventType, integrationEvent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add integration event {EventType} with ID {EventId} to outbox",
                integrationEvent.EventType, integrationEvent.Id);
            throw;
        }
    }

    public async Task AddEventsAsync(IEnumerable<IIntegrationEvent> integrationEvents, CancellationToken cancellationToken = default)
    {
        var events = integrationEvents.ToList();
        if (!events.Any())
        {
            return;
        }

        try
        {
            var outboxEvents = events.Select(evt =>
            {
                var eventData = JsonSerializer.Serialize(evt, evt.GetType());
                return new OutboxEvent(evt.Id, evt.EventType, eventData);
            });

            await _outboxRepository.AddRangeAsync(outboxEvents, cancellationToken);

            _logger.LogInformation("Added {Count} integration events to outbox", events.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add {Count} integration events to outbox", events.Count);
            throw;
        }
    }

    public async Task ProcessPendingEventsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var pendingEvents = await _outboxRepository.FindAsync(
                e => !e.IsProcessed && e.ProcessingAttempts == 0,
                cancellationToken);

            foreach (var outboxEvent in pendingEvents)
            {
                await ProcessEventAsync(outboxEvent, cancellationToken);
            }

            _logger.LogInformation("Processed {Count} pending outbox events", pendingEvents.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing pending outbox events");
            throw;
        }
    }

    public async Task RetryFailedEventsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var now = DateTime.UtcNow;
            var retryEvents = await _outboxRepository.FindAsync(
                e => !e.IsProcessed &&
                     e.ProcessingAttempts > 0 &&
                     e.NextRetry.HasValue &&
                     e.NextRetry.Value <= now,
                cancellationToken);

            foreach (var outboxEvent in retryEvents)
            {
                await ProcessEventAsync(outboxEvent, cancellationToken);
            }

            _logger.LogInformation("Retried {Count} failed outbox events", retryEvents.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying failed outbox events");
            throw;
        }
    }

    public async Task CleanupProcessedEventsAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        try
        {
            var oldProcessedEvents = await _outboxRepository.FindAsync(
                e => e.IsProcessed && e.ProcessedAt.HasValue && e.ProcessedAt.Value < olderThan,
                cancellationToken);

            foreach (var outboxEvent in oldProcessedEvents)
            {
                await _outboxRepository.RemoveByIdAsync(outboxEvent.Id, cancellationToken);
            }

            _logger.LogInformation("Cleaned up {Count} old processed outbox events", oldProcessedEvents.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old processed outbox events");
            throw;
        }
    }

    public async Task<OutboxStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var allEvents = await _outboxRepository.GetAllAsync(cancellationToken);
            var eventsList = allEvents.ToList();

            var now = DateTime.UtcNow;
            var statistics = new OutboxStatistics
            {
                TotalEvents = eventsList.Count,
                PendingEvents = eventsList.Count(e => !e.IsProcessed),
                ProcessedEvents = eventsList.Count(e => e.IsProcessed),
                FailedEvents = eventsList.Count(e => !e.IsProcessed && e.ProcessingAttempts > 0),
                RetryReadyEvents = eventsList.Count(e => e.IsReadyForRetry()),
                OldestPendingEvent = eventsList.Where(e => !e.IsProcessed).Min(e => (DateTime?)e.CreatedAt),
                NewestProcessedEvent = eventsList.Where(e => e.IsProcessed && e.ProcessedAt.HasValue).Max(e => e.ProcessedAt)
            };

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting outbox statistics");
            throw;
        }
    }

    private async Task ProcessEventAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken)
    {
        try
        {
            var eventType = Type.GetType(outboxEvent.EventType);
            if (eventType == null)
            {
                throw new InvalidOperationException($"Cannot find type {outboxEvent.EventType}");
            }

            var integrationEvent = JsonSerializer.Deserialize(outboxEvent.EventData, eventType);
            if (integrationEvent == null)
            {
                throw new InvalidOperationException($"Failed to deserialize event data for type {outboxEvent.EventType}");
            }

            await _eventBus.PublishAsync((IIntegrationEvent)integrationEvent, cancellationToken);

            outboxEvent.MarkAsProcessed();
            await _outboxRepository.UpdateAsync(outboxEvent, cancellationToken);

            _logger.LogInformation("Successfully processed outbox event {EventId} of type {EventType}",
                outboxEvent.EventId, outboxEvent.EventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process outbox event {EventId} of type {EventType}",
                outboxEvent.EventId, outboxEvent.EventType);

            outboxEvent.MarkAsFailed(ex.Message);
            await _outboxRepository.UpdateAsync(outboxEvent, cancellationToken);
        }
    }
}
