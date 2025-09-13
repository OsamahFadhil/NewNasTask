namespace Insight.Invoicing.Shared.Common;

public abstract class BaseEntity : IBaseEntity
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;

    protected void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}

