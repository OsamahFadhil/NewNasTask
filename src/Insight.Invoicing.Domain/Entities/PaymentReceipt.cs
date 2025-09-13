using Insight.Invoicing.Domain.Enums;
using Insight.Invoicing.Domain.Events;
using Insight.Invoicing.Domain.Exceptions;
using Insight.Invoicing.Domain.ValueObjects;
using Insight.Invoicing.Shared.Common;

namespace Insight.Invoicing.Domain.Entities;

public class PaymentReceipt : AggregateRoot
{
    public int ContractId { get; private set; }

    public int InstallmentId { get; private set; }

    public Money AmountPaid { get; private set; } = null!;

    public DateTime PaymentDate { get; private set; }

    public PaymentReceiptStatus Status { get; private set; } = PaymentReceiptStatus.PendingValidation;

    public FileReference FileReference { get; private set; } = null!;

    public string? Comments { get; private set; }

    public int? ValidatedBy { get; private set; }

    public DateTime? ValidatedAt { get; private set; }

    public Contract Contract { get; private set; } = null!;

    public Installment Installment { get; private set; } = null!;

    private PaymentReceipt() { }

    public PaymentReceipt(
        int contractId,
        int installmentId,
        Money amountPaid,
        DateTime paymentDate,
        FileReference fileReference)
    {
        if (contractId <= 0)
            throw new ArgumentException("Contract ID must be greater than zero", nameof(contractId));

        if (installmentId <= 0)
            throw new ArgumentException("Installment ID must be greater than zero", nameof(installmentId));

        if (amountPaid == null)
            throw new ArgumentNullException(nameof(amountPaid));

        if (amountPaid.Amount <= 0)
            throw new InvalidPaymentAmountException("Payment amount must be greater than zero");

        if (paymentDate > DateTime.UtcNow)
            throw new ArgumentException("Payment date cannot be in the future", nameof(paymentDate));

        ContractId = contractId;
        InstallmentId = installmentId;
        AmountPaid = amountPaid;
        PaymentDate = paymentDate.Date;
        FileReference = fileReference ?? throw new ArgumentNullException(nameof(fileReference));
    }

    public void Upload(int tenantId)
    {
        if (Status != PaymentReceiptStatus.PendingValidation)
            throw new InvalidOperationException("Receipt can only be uploaded when pending validation");

        AddDomainEvent(new PaymentReceiptUploadedEvent(
            Id,
            ContractId,
            InstallmentId,
            tenantId,
            AmountPaid.Amount,
            FileReference.OriginalFileName));
    }

    public void Approve(int approvedBy, string? comments = null)
    {
        if (Status != PaymentReceiptStatus.PendingValidation)
            throw new InvalidOperationException($"Receipt cannot be approved from {Status} status");

        Status = PaymentReceiptStatus.Approved;
        ValidatedBy = approvedBy;
        ValidatedAt = DateTime.UtcNow;
        Comments = comments;
        UpdateTimestamp();

        AddDomainEvent(new PaymentReceiptApprovedEvent(
            Id,
            ContractId,
            InstallmentId,
            0, // Will be resolved in the event handler
            approvedBy,
            AmountPaid.Amount));
    }

    public void Reject(int rejectedBy, string reason)
    {
        if (Status != PaymentReceiptStatus.PendingValidation)
            throw new InvalidOperationException($"Receipt cannot be rejected from {Status} status");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Rejection reason cannot be null or empty", nameof(reason));

        Status = PaymentReceiptStatus.Rejected;
        ValidatedBy = rejectedBy;
        ValidatedAt = DateTime.UtcNow;
        Comments = reason;
        UpdateTimestamp();

        AddDomainEvent(new PaymentReceiptRejectedEvent(
            Id,
            ContractId,
            0, // Will be resolved in the event handler
            rejectedBy,
            reason));
    }

    public void UpdateAmount(Money newAmount)
    {
        if (Status != PaymentReceiptStatus.PendingValidation)
            throw new InvalidOperationException("Amount can only be updated for pending receipts");

        if (newAmount == null)
            throw new ArgumentNullException(nameof(newAmount));

        if (newAmount.Amount <= 0)
            throw new InvalidPaymentAmountException("Payment amount must be greater than zero");

        AmountPaid = newAmount;
        UpdateTimestamp();
    }

    public void UpdatePaymentDate(DateTime newPaymentDate)
    {
        if (Status != PaymentReceiptStatus.PendingValidation)
            throw new InvalidOperationException("Payment date can only be updated for pending receipts");

        if (newPaymentDate > DateTime.UtcNow)
            throw new ArgumentException("Payment date cannot be in the future", nameof(newPaymentDate));

        PaymentDate = newPaymentDate.Date;
        UpdateTimestamp();
    }

    public bool IsImageReceipt() => FileReference.IsImage();

    public bool IsPdfReceipt() => FileReference.IsPdf();

    public string GetFileSizeString() => FileReference.GetFileSizeString();

    public bool IsValidated() => Status != PaymentReceiptStatus.PendingValidation;

    public string GetStatusDescription()
    {
        return Status switch
        {
            PaymentReceiptStatus.PendingValidation => "Pending Validation",
            PaymentReceiptStatus.Approved => "Approved",
            PaymentReceiptStatus.Rejected => "Rejected",
            _ => "Unknown"
        };
    }
}

