using Insight.Invoicing.Application.Commands.PaymentReceipts;
using Insight.Invoicing.Application.DTOs;
using Insight.Invoicing.Domain.Entities;
using Insight.Invoicing.Domain.Repositories;
using Insight.Invoicing.Domain.ValueObjects;
using MediatR;

namespace Insight.Invoicing.Application.Handlers.PaymentReceipts;

public class UploadPaymentReceiptCommandHandler : IRequestHandler<UploadPaymentReceiptCommand, PaymentReceiptDto>
{
    private readonly IPaymentReceiptRepository _paymentReceiptRepository;

    public UploadPaymentReceiptCommandHandler(IPaymentReceiptRepository paymentReceiptRepository)
    {
        _paymentReceiptRepository = paymentReceiptRepository;
    }

    public async Task<PaymentReceiptDto> Handle(UploadPaymentReceiptCommand request, CancellationToken cancellationToken)
    {
        var fileReference = new FileReference(
            request.FileName,
            request.ContentType,
            "bucket-name", // TODO: Get from configuration
            request.FileContent.Length,
            "object-key"); // TODO: Generate unique object key

        var paymentReceipt = new PaymentReceipt(
            request.ContractId,
            request.InstallmentId,
            Money.Usd(request.AmountPaid),
            request.PaymentDate,
            fileReference);

        var createdReceipt = await _paymentReceiptRepository.AddAsync(paymentReceipt, cancellationToken);

        return new PaymentReceiptDto(
            createdReceipt.Id,
            createdReceipt.ContractId,
            createdReceipt.InstallmentId,
            createdReceipt.AmountPaid.Amount,
            createdReceipt.PaymentDate,
            createdReceipt.Status,
            createdReceipt.FileReference.BucketName,
            createdReceipt.FileReference.ObjectName,
            createdReceipt.FileReference.OriginalFileName,
            createdReceipt.FileReference.FileSize,
            createdReceipt.FileReference.ContentType,
            createdReceipt.Comments,
            createdReceipt.ValidatedBy,
            null, // ValidatedByName - TODO: Resolve from user service
            createdReceipt.ValidatedAt,
            createdReceipt.CreatedAt,
            createdReceipt.UpdatedAt);
    }
}
