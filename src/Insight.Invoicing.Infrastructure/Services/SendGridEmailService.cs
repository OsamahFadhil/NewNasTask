using Insight.Invoicing.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Insight.Invoicing.Infrastructure.Services;

public class SendGridEmailService : IEmailService
{
    private readonly ISendGridClient _sendGridClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SendGridEmailService> _logger;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public SendGridEmailService(
        ISendGridClient sendGridClient,
        IConfiguration configuration,
        ILogger<SendGridEmailService> logger)
    {
        _sendGridClient = sendGridClient;
        _configuration = configuration;
        _logger = logger;
        _fromEmail = configuration["SendGrid:FromEmail"] ?? "noreply@insight-invoicing.com";
        _fromName = configuration["SendGrid:FromName"] ?? "Insight Invoicing System";
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
        var subject = "Contract Approved - Insight Invoicing";
        var htmlBody = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2 style='color: #28a745;'>Contract Approved!</h2>
                <p>Dear {tenantName},</p>
                <p>We're pleased to inform you that your contract (ID: {contractId}) has been approved.</p>
                
                <h3>Contract Details:</h3>
                <ul>
                    <li><strong>Contract ID:</strong> {contractId}</li>
                    <li><strong>Total Amount:</strong> {totalAmount:C}</li>
                    <li><strong>Number of Installments:</strong> {numberOfInstallments}</li>
                    <li><strong>Start Date:</strong> {startDate:yyyy-MM-dd}</li>
                    <li><strong>End Date:</strong> {endDate:yyyy-MM-dd}</li>
                </ul>
                
                <p>Your installment schedule has been generated and you can view it in your tenant portal.</p>
                
                <p>Best regards,<br>
                Insight Invoicing Team</p>
            </body>
            </html>";

        var plainTextBody = $@"
            Contract Approved!
            
            Dear {tenantName},
            
            We're pleased to inform you that your contract (ID: {contractId}) has been approved.
            
            Contract Details:
            - Contract ID: {contractId}
            - Total Amount: {totalAmount:C}
            - Number of Installments: {numberOfInstallments}
            - Start Date: {startDate:yyyy-MM-dd}
            - End Date: {endDate:yyyy-MM-dd}
            
            Your installment schedule has been generated and you can view it in your tenant portal.
            
            Best regards,
            Insight Invoicing Team";

        await SendEmailAsync(toEmail, subject, htmlBody, plainTextBody, cancellationToken);
    }

    public async Task SendContractRejectedEmailAsync(
        string toEmail,
        string tenantName,
        int contractId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var subject = "Contract Rejected - Insight Invoicing";
        var htmlBody = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2 style='color: #dc3545;'>Contract Rejected</h2>
                <p>Dear {tenantName},</p>
                <p>We regret to inform you that your contract (ID: {contractId}) has been rejected.</p>
                
                <h3>Reason for rejection:</h3>
                <p style='background-color: #f8f9fa; padding: 10px; border-left: 4px solid #dc3545;'>{reason}</p>
                
                <p>Please review the feedback and resubmit your contract with the necessary corrections.</p>
                
                <p>If you have any questions, please contact our support team.</p>
                
                <p>Best regards,<br>
                Insight Invoicing Team</p>
            </body>
            </html>";

        await SendEmailAsync(toEmail, subject, htmlBody, cancellationToken: cancellationToken);
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
        var adminEmails = _configuration.GetSection("AdminEmails").Get<string[]>() ?? Array.Empty<string>();

        if (adminEmails.Any())
        {
            var subject = "New Payment Receipt Uploaded - Insight Invoicing";
            var htmlBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2 style='color: #007bff;'>New Payment Receipt Uploaded</h2>
                    <p>A new payment receipt has been uploaded and requires validation.</p>
                    
                    <h3>Details:</h3>
                    <ul>
                        <li><strong>Tenant:</strong> {tenantName} ({tenantEmail})</li>
                        <li><strong>Amount:</strong> {amount:C}</li>
                        <li><strong>Payment Date:</strong> {paymentDate:yyyy-MM-dd}</li>
                        <li><strong>File Name:</strong> {fileName}</li>
                        <li><strong>Contract ID:</strong> {contractId}</li>
                        <li><strong>Installment ID:</strong> {installmentId}</li>
                    </ul>
                    
                    <p>Please log into the admin portal to review and validate this receipt.</p>
                    
                    <p>Best regards,<br>
                    Insight Invoicing System</p>
                </body>
                </html>";

            await SendBulkEmailAsync(adminEmails, subject, htmlBody, cancellationToken: cancellationToken);
        }
    }

    public async Task SendPaymentReceiptApprovedEmailAsync(
        string toEmail,
        string tenantName,
        decimal amount,
        int contractId,
        int installmentId,
        CancellationToken cancellationToken = default)
    {
        var subject = "Payment Receipt Approved - Insight Invoicing";
        var htmlBody = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2 style='color: #28a745;'>Payment Receipt Approved!</h2>
                <p>Dear {tenantName},</p>
                <p>Your payment receipt for {amount:C} has been approved and processed.</p>
                
                <h3>Payment Details:</h3>
                <ul>
                    <li><strong>Amount:</strong> {amount:C}</li>
                    <li><strong>Contract ID:</strong> {contractId}</li>
                    <li><strong>Installment ID:</strong> {installmentId}</li>
                </ul>
                
                <p>Your installment has been marked as paid. Thank you for your payment!</p>
                
                <p>Best regards,<br>
                Insight Invoicing Team</p>
            </body>
            </html>";

        await SendEmailAsync(toEmail, subject, htmlBody, cancellationToken: cancellationToken);
    }

    public async Task SendPaymentReceiptRejectedEmailAsync(
        string toEmail,
        string tenantName,
        decimal amount,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var subject = "Payment Receipt Rejected - Insight Invoicing";
        var htmlBody = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2 style='color: #dc3545;'>Payment Receipt Rejected</h2>
                <p>Dear {tenantName},</p>
                <p>We regret to inform you that your payment receipt for {amount:C} has been rejected.</p>
                
                <h3>Reason for rejection:</h3>
                <p style='background-color: #f8f9fa; padding: 10px; border-left: 4px solid #dc3545;'>{reason}</p>
                
                <p>Please upload a new receipt with the necessary corrections.</p>
                
                <p>Best regards,<br>
                Insight Invoicing Team</p>
            </body>
            </html>";

        await SendEmailAsync(toEmail, subject, htmlBody, cancellationToken: cancellationToken);
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
        var subject = $"Overdue Payment Reminder - {daysOverdue} Days - Insight Invoicing";
        var htmlBody = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2 style='color: #dc3545;'>Overdue Payment Reminder</h2>
                <p>Dear {tenantName},</p>
                <p>This is a reminder that your payment is overdue by {daysOverdue} days.</p>
                
                <h3>Payment Details:</h3>
                <ul>
                    <li><strong>Original Amount:</strong> {amount:C}</li>
                    <li><strong>Penalty Applied:</strong> {penaltyAmount:C}</li>
                    <li><strong>Total Due:</strong> {(amount + penaltyAmount):C}</li>
                    <li><strong>Due Date:</strong> {dueDate:yyyy-MM-dd}</li>
                    <li><strong>Contract ID:</strong> {contractId}</li>
                    <li><strong>Installment ID:</strong> {installmentId}</li>
                </ul>
                
                <p style='color: #dc3545;'><strong>Please make payment as soon as possible to avoid additional penalties.</strong></p>
                
                <p>Best regards,<br>
                Insight Invoicing Team</p>
            </body>
            </html>";

        await SendEmailAsync(toEmail, subject, htmlBody, cancellationToken: cancellationToken);
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
        var subject = $"Payment Due Reminder - {daysUntilDue} Days - Insight Invoicing";
        var htmlBody = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2 style='color: #ffc107;'>Payment Due Reminder</h2>
                <p>Dear {tenantName},</p>
                <p>This is a friendly reminder that your payment is due in {daysUntilDue} days.</p>
                
                <h3>Payment Details:</h3>
                <ul>
                    <li><strong>Amount Due:</strong> {amount:C}</li>
                    <li><strong>Due Date:</strong> {dueDate:yyyy-MM-dd}</li>
                    <li><strong>Contract ID:</strong> {contractId}</li>
                    <li><strong>Installment ID:</strong> {installmentId}</li>
                </ul>
                
                <p>Please ensure payment is made by the due date to avoid any penalty charges.</p>
                
                <p>Best regards,<br>
                Insight Invoicing Team</p>
            </body>
            </html>";

        await SendEmailAsync(toEmail, subject, htmlBody, cancellationToken: cancellationToken);
    }

    public async Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextBody, htmlBody);

            var response = await _sendGridClient.SendEmailAsync(msg, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully to {ToEmail} with subject '{Subject}'", toEmail, subject);
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send email to {ToEmail}. Status: {StatusCode}, Body: {Body}",
                    toEmail, response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {ToEmail} with subject '{Subject}'", toEmail, subject);
            throw;
        }
    }

    public async Task SendBulkEmailAsync(
        IEnumerable<string> toEmails,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken cancellationToken = default)
    {
        var tasks = toEmails.Select(email => SendEmailAsync(email, subject, htmlBody, plainTextBody, cancellationToken));
        await Task.WhenAll(tasks);
    }
}


