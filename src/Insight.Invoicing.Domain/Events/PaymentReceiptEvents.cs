using Insight.Invoicing.Shared.Common;

namespace Insight.Invoicing.Domain.Events;

public record PaymentReceiptUploadedEvent(
    int PaymentReceiptId,
    int ContractId,
    int InstallmentId,
    int TenantId,
    decimal AmountPaid,
    string FileName
) : DomainEvent;

public record PaymentReceiptApprovedEvent(
    int PaymentReceiptId,
    int ContractId,
    int InstallmentId,
    int TenantId,
    int ApprovedBy,
    decimal AmountPaid
) : DomainEvent;

public record PaymentReceiptRejectedEvent(
    int PaymentReceiptId,
    int ContractId,
    int TenantId,
    int RejectedBy,
    string Reason
) : DomainEvent;

