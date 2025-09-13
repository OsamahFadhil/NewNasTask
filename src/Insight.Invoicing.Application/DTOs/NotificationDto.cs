namespace Insight.Invoicing.Application.DTOs;

public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public int? UserId { get; set; }
    public int? ContractId { get; set; }
    public int? InstallmentId { get; set; }
    public int? PaymentReceiptId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public Dictionary<string, object> Data { get; set; } = new();
}

public enum NotificationType
{
    ContractSubmitted,
    ContractApproved,
    ContractRejected,
    ContractUpdateRequested,
    InstallmentDue,
    InstallmentOverdue,
    PaymentReceiptUploaded,
    PaymentReceiptApproved,
    PaymentReceiptRejected,
    PenaltyApplied,
    GeneralInfo,
    SystemAlert
}

public class RealTimeNotificationDto
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? ActionUrl { get; set; }
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
}

public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Critical
}
