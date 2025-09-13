using Insight.Invoicing.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Insight.Invoicing.Infrastructure.EventHandlers;

public class PaymentReceiptUploadedEventHandler : INotificationHandler<PaymentReceiptUploadedEvent>
{
    private readonly ILogger<PaymentReceiptUploadedEventHandler> _logger;

    public PaymentReceiptUploadedEventHandler(ILogger<PaymentReceiptUploadedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(PaymentReceiptUploadedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing PaymentReceiptUploadedEvent for receipt {PaymentReceiptId}",
            notification.PaymentReceiptId);

        try
        {
            await SendAdminNotificationAsync(notification, cancellationToken);

            _logger.LogInformation(
                "Successfully processed PaymentReceiptUploadedEvent for receipt {PaymentReceiptId}",
                notification.PaymentReceiptId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing PaymentReceiptUploadedEvent for receipt {PaymentReceiptId}",
                notification.PaymentReceiptId);
            throw;
        }
    }

    private async Task SendAdminNotificationAsync(
        PaymentReceiptUploadedEvent notification,
        CancellationToken cancellationToken)
    {

        _logger.LogInformation(
            "Would send admin notification for payment receipt {PaymentReceiptId} - " +
            "Tenant {TenantId} uploaded receipt for contract {ContractId}, installment {InstallmentId}, " +
            "amount {AmountPaid:C}, file {FileName}",
            notification.PaymentReceiptId,
            notification.TenantId,
            notification.ContractId,
            notification.InstallmentId,
            notification.AmountPaid,
            notification.FileName);

        // {
        //     await _emailService.SendPaymentReceiptNotificationAsync(admin.Email, notification);
        // }

        await Task.CompletedTask;
    }
}

