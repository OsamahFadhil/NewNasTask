namespace Insight.Invoicing.Shared.Common;

public interface IBaseEntity
{
    int Id { get; }

    DateTime CreatedAt { get; }

    DateTime UpdatedAt { get; }
}


