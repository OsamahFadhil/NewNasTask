using Insight.Invoicing.Application.Services;
using Microsoft.Extensions.Logging;

namespace Insight.Invoicing.Infrastructure.Services;

public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendContractApprovedEmailAsync(
        string toEmail,
        string tenantName,
        int contractId,
        decimal totalAmount,
        int numberOfInstallments,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MOCK EMAIL: Contract approved notification sent to {Email} for tenant {TenantName}, Contract ID: {ContractId}, Amount: {Amount:C}",
            toEmail, tenantName, contractId, totalAmount);

        await Task.Delay(100, cancellationToken); // Simulate network delay
    }

    public async Task SendContractRejectedEmailAsync(
        string toEmail,
        string tenantName,
        int contractId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MOCK EMAIL: Contract rejected notification sent to {Email} for tenant {TenantName}, Contract ID: {ContractId}, Reason: {Reason}",
            toEmail, tenantName, contractId, reason);

        await Task.Delay(100, cancellationToken);
    }

    public async Task SendPaymentReceiptUploadedEmailAsync(
        string tenantName,
        string tenantEmail,
        decimal amount,
        string fileName,
        DateTime paymentDate,
        int contractId,
        int installmentId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MOCK EMAIL: Payment receipt uploaded notification sent to administrators. Tenant: {TenantName} ({Email}), Amount: {Amount:C}, File: {FileName}",
            tenantName, tenantEmail, amount, fileName);

        await Task.Delay(100, cancellationToken);
    }

    public async Task SendPaymentReceiptApprovedEmailAsync(
        string toEmail,
        string tenantName,
        decimal amount,
        int contractId,
        int installmentId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MOCK EMAIL: Payment receipt approved notification sent to {Email} for tenant {TenantName}, Amount: {Amount:C}",
            toEmail, tenantName, amount);

        await Task.Delay(100, cancellationToken);
    }

    public async Task SendPaymentReceiptRejectedEmailAsync(
        string toEmail,
        string tenantName,
        decimal amount,
        string reason,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MOCK EMAIL: Payment receipt rejected notification sent to {Email} for tenant {TenantName}, Amount: {Amount:C}, Reason: {Reason}",
            toEmail, tenantName, amount, reason);

        await Task.Delay(100, cancellationToken);
    }

    public async Task SendInstallmentOverdueReminderAsync(
        string toEmail,
        string tenantName,
        decimal amount,
        decimal penaltyAmount,
        DateTime dueDate,
        int daysOverdue,
        int contractId,
        int installmentId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("MOCK EMAIL: Overdue reminder sent to {Email} for tenant {TenantName}, Amount: {Amount:C}, Penalty: {Penalty:C}, Days overdue: {DaysOverdue}",
            toEmail, tenantName, amount, penaltyAmount, daysOverdue);

        await Task.Delay(100, cancellationToken);
    }

    public async Task SendInstallmentDueReminderAsync(
        string toEmail,
        string tenantName,
        decimal amount,
        DateTime dueDate,
        int daysUntilDue,
        int contractId,
        int installmentId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MOCK EMAIL: Due reminder sent to {Email} for tenant {TenantName}, Amount: {Amount:C}, Days until due: {DaysUntilDue}",
            toEmail, tenantName, amount, daysUntilDue);

        await Task.Delay(100, cancellationToken);
    }

    public async Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MOCK EMAIL: Generic email sent to {Email} with subject '{Subject}'",
            toEmail, subject);

        await Task.Delay(100, cancellationToken);
    }

    public async Task SendBulkEmailAsync(
        IEnumerable<string> toEmails,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken cancellationToken = default)
    {
        var emails = toEmails.ToList();
        _logger.LogInformation("MOCK EMAIL: Bulk email sent to {Count} recipients with subject '{Subject}'",
            emails.Count, subject);

        await Task.Delay(200, cancellationToken);
    }
}


