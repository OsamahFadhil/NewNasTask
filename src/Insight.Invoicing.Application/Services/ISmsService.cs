namespace Insight.Invoicing.Application.Services;

public interface ISmsService
{
    Task<SmsResult> SendSmsAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<SmsResult>> SendBulkSmsAsync(
        IEnumerable<string> phoneNumbers,
        string message,
        CancellationToken cancellationToken = default);

    Task SendContractApprovedSmsAsync(
        string phoneNumber,
        string tenantName,
        int contractId,
        CancellationToken cancellationToken = default);

    Task SendInstallmentOverdueSmsAsync(
        string phoneNumber,
        string tenantName,
        decimal amount,
        DateTime dueDate,
        int daysOverdue,
        CancellationToken cancellationToken = default);

    Task SendInstallmentDueSmsAsync(
        string phoneNumber,
        string tenantName,
        decimal amount,
        DateTime dueDate,
        int daysUntilDue,
        CancellationToken cancellationToken = default);

    Task SendPaymentReceiptApprovedSmsAsync(
        string phoneNumber,
        string tenantName,
        decimal amount,
        CancellationToken cancellationToken = default);
}

public class SmsResult
{
    public bool IsSuccess { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}


