using MediatR;

namespace Insight.Invoicing.Shared.Common;

public interface IDomainEvent : INotification
{
    DateTime OccurredAt { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
