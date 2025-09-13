using Insight.Invoicing.Application.IntegrationEvents;
using Insight.Invoicing.Application.Services;
using Insight.Invoicing.Application.DTOs;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Insight.Invoicing.Infrastructure.EventHandlers.IntegrationEventConsumers;

public class PaymentReceiptUploadedConsumer : IConsumer<PaymentReceiptUploadedIntegrationEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly ILogger<PaymentReceiptUploadedConsumer> _logger;

    public PaymentReceiptUploadedConsumer(
        INotificationService notificationService,
        IEmailService emailService,
        ILogger<PaymentReceiptUploadedConsumer> logger)
    {
        _notificationService = notificationService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentReceiptUploadedIntegrationEvent> context)
    {
        var integrationEvent = context.Message;

        _logger.LogInformation("Processing PaymentReceiptUploadedIntegrationEvent for receipt {PaymentReceiptId}",
            integrationEvent.PaymentReceiptId);

        try
        {
            var adminNotification = new RealTimeNotificationDto
            {
                Type = NotificationType.PaymentReceiptUploaded.ToString(),
                Title = "New Payment Receipt",
                Message = $"New payment receipt uploaded by {integrationEvent.TenantName} for amount {integrationEvent.AmountPaid:C}",
                Priority = NotificationPriority.Normal,
                Data = new
                {
                    receiptId = integrationEvent.PaymentReceiptId,
                    contractId = integrationEvent.ContractId,
                    tenantName = integrationEvent.TenantName,
                    amount = integrationEvent.AmountPaid
                },
                ActionUrl = $"/admin/receipts/{integrationEvent.PaymentReceiptId}"
            };

            await _notificationService.SendToAdministratorsAsync(adminNotification);

            var tenantNotification = new RealTimeNotificationDto
            {
                Type = NotificationType.PaymentReceiptUploaded.ToString(),
                Title = "Receipt Uploaded",
                Message = $"Your payment receipt for {integrationEvent.AmountPaid:C} has been uploaded and is pending validation.",
                Priority = NotificationPriority.Normal,
                Data = new
                {
                    receiptId = integrationEvent.PaymentReceiptId,
                    contractId = integrationEvent.ContractId,
                    amount = integrationEvent.AmountPaid
                }
            };

            await _notificationService.SendToTenantAsync(integrationEvent.TenantId, tenantNotification);

            await _emailService.SendPaymentReceiptUploadedEmailAsync(
                integrationEvent.TenantName,
                integrationEvent.TenantEmail,
                integrationEvent.AmountPaid,
                integrationEvent.FileName,
                integrationEvent.PaymentDate,
                integrationEvent.ContractId,
                integrationEvent.InstallmentId);

            _logger.LogInformation("Successfully processed PaymentReceiptUploadedIntegrationEvent for receipt {PaymentReceiptId}",
                integrationEvent.PaymentReceiptId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PaymentReceiptUploadedIntegrationEvent for receipt {PaymentReceiptId}",
                integrationEvent.PaymentReceiptId);
            throw;
        }
    }
}


