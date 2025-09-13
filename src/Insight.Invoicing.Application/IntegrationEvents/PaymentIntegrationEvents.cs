namespace Insight.Invoicing.Application.IntegrationEvents;

public record PaymentReceiptUploadedIntegrationEvent(
    int PaymentReceiptId,
    int ContractId,
    int InstallmentId,
    int TenantId,
    string TenantEmail,
    string TenantName,
    decimal AmountPaid,
    string FileName,
    DateTime PaymentDate
) : IntegrationEvent;

public record PaymentReceiptApprovedIntegrationEvent(
    int PaymentReceiptId,
    int ContractId,
    int InstallmentId,
    int TenantId,
    string TenantEmail,
    string TenantName,
    int ApprovedBy,
    string ApprovedByName,
    decimal AmountPaid
) : IntegrationEvent;

public record PaymentReceiptRejectedIntegrationEvent(
    int PaymentReceiptId,
    int ContractId,
    int InstallmentId,
    int TenantId,
    string TenantEmail,
    string TenantName,
    int RejectedBy,
    string RejectedByName,
    string Reason
) : IntegrationEvent;

public record InstallmentOverdueIntegrationEvent(
    int InstallmentId,
    int ContractId,
    int TenantId,
    string TenantEmail,
    string TenantName,
    decimal Amount,
    decimal PenaltyAmount,
    DateTime DueDate,
    int DaysOverdue
) : IntegrationEvent;

public record PenaltyAppliedIntegrationEvent(
    int InstallmentId,
    int ContractId,
    int TenantId,
    string TenantEmail,
    string TenantName,
    decimal PenaltyAmount,
    decimal TotalAmountDue,
    DateTime AppliedAt
) : IntegrationEvent;


