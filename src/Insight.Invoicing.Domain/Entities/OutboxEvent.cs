using Insight.Invoicing.Shared.Common;

namespace Insight.Invoicing.Domain.Entities;

public class OutboxEvent : BaseEntity
{
    public Guid EventId { get; private set; }

    public string EventType { get; private set; } = string.Empty;

    public string EventData { get; private set; } = string.Empty;

    public bool IsProcessed { get; private set; }

    public DateTime? ProcessedAt { get; private set; }

    public int ProcessingAttempts { get; private set; }

    public string? LastError { get; private set; }

    public DateTime? NextRetry { get; private set; }

    public int MaxRetryAttempts { get; private set; } = 3;

    private OutboxEvent() { }

    public OutboxEvent(Guid eventId, string eventType, string eventData, int maxRetryAttempts = 3)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException("Event ID cannot be empty", nameof(eventId));

        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Event type cannot be null or empty", nameof(eventType));

        if (string.IsNullOrWhiteSpace(eventData))
            throw new ArgumentException("Event data cannot be null or empty", nameof(eventData));

        EventId = eventId;
        EventType = eventType.Trim();
        EventData = eventData.Trim();
        IsProcessed = false;
        ProcessingAttempts = 0;
        MaxRetryAttempts = maxRetryAttempts;
    }

    public void MarkAsProcessed()
    {
        IsProcessed = true;
        ProcessedAt = DateTime.UtcNow;
        LastError = null;
        NextRetry = null;
        UpdateTimestamp();
    }

    public void MarkAsFailed(string error)
    {
        ProcessingAttempts++;
        LastError = error;

        if (ProcessingAttempts < MaxRetryAttempts)
        {
            var delayMinutes = Math.Pow(2, ProcessingAttempts);
            NextRetry = DateTime.UtcNow.AddMinutes(delayMinutes);
        }
        else
        {
            NextRetry = null;
        }

        UpdateTimestamp();
    }

    public bool IsReadyForRetry()
    {
        return !IsProcessed &&
               ProcessingAttempts < MaxRetryAttempts &&
               NextRetry.HasValue &&
               NextRetry.Value <= DateTime.UtcNow;
    }

    public bool HasExceededMaxRetries()
    {
        return !IsProcessed && ProcessingAttempts >= MaxRetryAttempts;
    }
}


