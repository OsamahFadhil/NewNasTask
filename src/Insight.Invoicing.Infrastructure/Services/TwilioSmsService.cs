using Insight.Invoicing.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Insight.Invoicing.Infrastructure.Services;

public class TwilioSmsService : ISmsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TwilioSmsService> _logger;
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _fromPhoneNumber;

    public TwilioSmsService(IConfiguration configuration, ILogger<TwilioSmsService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _accountSid = configuration["Twilio:AccountSid"] ?? throw new InvalidOperationException("Twilio AccountSid not configured");
        _authToken = configuration["Twilio:AuthToken"] ?? throw new InvalidOperationException("Twilio AuthToken not configured");
        _fromPhoneNumber = configuration["Twilio:FromPhoneNumber"] ?? throw new InvalidOperationException("Twilio FromPhoneNumber not configured");

        TwilioClient.Init(_accountSid, _authToken);
    }

    public async Task<SmsResult> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new Twilio.Types.PhoneNumber(_fromPhoneNumber),
                to: new Twilio.Types.PhoneNumber(phoneNumber));

            _logger.LogInformation("SMS sent successfully to {PhoneNumber} with SID {MessageSid}", phoneNumber, messageResource.Sid);

            return new SmsResult
            {
                IsSuccess = true,
                MessageId = messageResource.Sid,
                PhoneNumber = phoneNumber,
                SentAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);

            return new SmsResult
            {
                IsSuccess = false,
                PhoneNumber = phoneNumber,
                ErrorMessage = ex.Message,
                SentAt = DateTime.UtcNow
            };
        }
    }

    public async Task<IEnumerable<SmsResult>> SendBulkSmsAsync(IEnumerable<string> phoneNumbers, string message, CancellationToken cancellationToken = default)
    {
        var tasks = phoneNumbers.Select(phone => SendSmsAsync(phone, message, cancellationToken));
        var results = await Task.WhenAll(tasks);
        return results;
    }

    public async Task SendContractApprovedSmsAsync(string phoneNumber, string tenantName, int contractId, CancellationToken cancellationToken = default)
    {
        var message = $"Hi {tenantName}, your contract #{contractId} has been approved! You can now view your installment schedule in the tenant portal.";
        await SendSmsAsync(phoneNumber, message, cancellationToken);
    }

    public async Task SendInstallmentOverdueSmsAsync(string phoneNumber, string tenantName, decimal amount, DateTime dueDate, int daysOverdue, CancellationToken cancellationToken = default)
    {
        var message = $"URGENT: Hi {tenantName}, your payment of {amount:C} was due on {dueDate:yyyy-MM-dd} and is now {daysOverdue} days overdue. Please pay immediately to avoid additional penalties.";
        await SendSmsAsync(phoneNumber, message, cancellationToken);
    }

    public async Task SendInstallmentDueSmsAsync(string phoneNumber, string tenantName, decimal amount, DateTime dueDate, int daysUntilDue, CancellationToken cancellationToken = default)
    {
        var message = $"Reminder: Hi {tenantName}, your payment of {amount:C} is due on {dueDate:yyyy-MM-dd} ({daysUntilDue} days). Please ensure timely payment. Thank you!";
        await SendSmsAsync(phoneNumber, message, cancellationToken);
    }

    public async Task SendPaymentReceiptApprovedSmsAsync(string phoneNumber, string tenantName, decimal amount, CancellationToken cancellationToken = default)
    {
        var message = $"Great news! Hi {tenantName}, your payment of {amount:C} has been approved and processed successfully. Thank you for your payment!";
        await SendSmsAsync(phoneNumber, message, cancellationToken);
    }
}


