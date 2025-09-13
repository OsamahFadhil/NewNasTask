using Insight.Invoicing.Application.DTOs;
using MediatR;

namespace Insight.Invoicing.Application.Commands.PaymentReceipts;

public record UploadPaymentReceiptCommand(
    int ContractId,
    int InstallmentId,
    decimal AmountPaid,
    DateTime PaymentDate,
    byte[] FileContent,
    string FileName,
    string ContentType,
    int TenantId
) : IRequest<PaymentReceiptDto>;

