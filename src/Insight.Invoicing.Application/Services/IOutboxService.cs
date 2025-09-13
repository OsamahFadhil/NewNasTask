using Insight.Invoicing.Application.IntegrationEvents;

namespace Insight.Invoicing.Application.Services;

public interface IOutboxService
{
    Task AddEventAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : class, IIntegrationEvent;

    Task AddEventsAsync(IEnumerable<IIntegrationEvent> integrationEvents, CancellationToken cancellationToken = default);

    Task ProcessPendingEventsAsync(CancellationToken cancellationToken = default);

    Task RetryFailedEventsAsync(CancellationToken cancellationToken = default);

    Task CleanupProcessedEventsAsync(DateTime olderThan, CancellationToken cancellationToken = default);

    Task<OutboxStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
}

public class OutboxStatistics
{
    public int TotalEvents { get; set; }
    public int PendingEvents { get; set; }
    public int ProcessedEvents { get; set; }
    public int FailedEvents { get; set; }
    public int RetryReadyEvents { get; set; }
    public DateTime? OldestPendingEvent { get; set; }
    public DateTime? NewestProcessedEvent { get; set; }
}


