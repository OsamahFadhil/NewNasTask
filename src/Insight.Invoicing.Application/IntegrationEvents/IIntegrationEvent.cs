namespace Insight.Invoicing.Application.IntegrationEvents;

public interface IIntegrationEvent
{
    Guid Id { get; }

    DateTime OccurredOn { get; }

    string EventType { get; }
}

public abstract record IntegrationEvent : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public string EventType { get; init; }

    protected IntegrationEvent()
    {
        EventType = GetType().Name;
    }
}


