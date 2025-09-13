using Insight.Invoicing.Application.Services;
using Microsoft.Extensions.Logging;

namespace Insight.Invoicing.Infrastructure.Services;

public class MockSmsService : ISmsService
{
    private readonly ILogger<MockSmsService> _logger;

    public MockSmsService(ILogger<MockSmsService> logger)
    {
        _logger = logger;
    }

    public async Task<SmsResult> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MOCK SMS: Sent to {PhoneNumber}, Message: {Message}", phoneNumber, message);

        await Task.Delay(100, cancellationToken); // Simulate network delay

        return new SmsResult
        {
            IsSuccess = true,
            MessageId = Guid.NewGuid().ToString(),
            PhoneNumber = phoneNumber,
            SentAt = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<SmsResult>> SendBulkSmsAsync(IEnumerable<string> phoneNumbers, string message, CancellationToken cancellationToken = default)
    {
        var numbers = phoneNumbers.ToList();
        _logger.LogInformation("MOCK SMS: Bulk SMS sent to {Count} recipients, Message: {Message}", numbers.Count, message);

        await Task.Delay(200, cancellationToken);

        return numbers.Select(phone => new SmsResult
        {
            IsSuccess = true,
            MessageId = Guid.NewGuid().ToString(),
            PhoneNumber = phone,
            SentAt = DateTime.UtcNow
        });
    }

    public async Task SendContractApprovedSmsAsync(string phoneNumber, string tenantName, int contractId, CancellationToken cancellationToken = default)
    {
        var message = $"Hi {tenantName}, your contract #{contractId} has been approved! You can now view your installment schedule.";
        await SendSmsAsync(phoneNumber, message, cancellationToken);
    }

    public async Task SendInstallmentOverdueSmsAsync(string phoneNumber, string tenantName, decimal amount, DateTime dueDate, int daysOverdue, CancellationToken cancellationToken = default)
    {
        var message = $"Hi {tenantName}, your payment of {amount:C} was due on {dueDate:yyyy-MM-dd} and is now {daysOverdue} days overdue. Please pay to avoid additional penalties.";
        await SendSmsAsync(phoneNumber, message, cancellationToken);
    }

    public async Task SendInstallmentDueSmsAsync(string phoneNumber, string tenantName, decimal amount, DateTime dueDate, int daysUntilDue, CancellationToken cancellationToken = default)
    {
        var message = $"Hi {tenantName}, reminder: payment of {amount:C} is due on {dueDate:yyyy-MM-dd} ({daysUntilDue} days). Thank you!";
        await SendSmsAsync(phoneNumber, message, cancellationToken);
    }

    public async Task SendPaymentReceiptApprovedSmsAsync(string phoneNumber, string tenantName, decimal amount, CancellationToken cancellationToken = default)
    {
        var message = $"Hi {tenantName}, your payment of {amount:C} has been approved and processed. Thank you!";
        await SendSmsAsync(phoneNumber, message, cancellationToken);
    }
}


