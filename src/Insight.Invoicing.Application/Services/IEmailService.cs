namespace Insight.Invoicing.Application.Services;

public interface IEmailService
{
    Task SendContractApprovedEmailAsync(
        string toEmail,
        string tenantName,
        int contractId,
        decimal totalAmount,
        int numberOfInstallments,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    Task SendContractRejectedEmailAsync(
        string toEmail,
        string tenantName,
        int contractId,
        string reason,
        CancellationToken cancellationToken = default);

    Task SendPaymentReceiptUploadedEmailAsync(
        string tenantName,
        string tenantEmail,
        decimal amount,
        string fileName,
        DateTime paymentDate,
        int contractId,
        int installmentId,
        CancellationToken cancellationToken = default);

    Task SendPaymentReceiptApprovedEmailAsync(
        string toEmail,
        string tenantName,
        decimal amount,
        int contractId,
        int installmentId,
        CancellationToken cancellationToken = default);

    Task SendPaymentReceiptRejectedEmailAsync(
        string toEmail,
        string tenantName,
        decimal amount,
        string reason,
        CancellationToken cancellationToken = default);

    Task SendInstallmentOverdueReminderAsync(
        string toEmail,
        string tenantName,
        decimal amount,
        decimal penaltyAmount,
        DateTime dueDate,
        int daysOverdue,
        int contractId,
        int installmentId,
        CancellationToken cancellationToken = default);

    Task SendInstallmentDueReminderAsync(
        string toEmail,
        string tenantName,
        decimal amount,
        DateTime dueDate,
        int daysUntilDue,
        int contractId,
        int installmentId,
        CancellationToken cancellationToken = default);

    Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken cancellationToken = default);

    Task SendBulkEmailAsync(
        IEnumerable<string> toEmails,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken cancellationToken = default);
}


