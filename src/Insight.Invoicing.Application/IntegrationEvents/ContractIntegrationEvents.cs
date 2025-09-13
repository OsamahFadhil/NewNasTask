namespace Insight.Invoicing.Application.IntegrationEvents;

public record ContractSubmittedIntegrationEvent(
    int ContractId,
    int TenantId,
    string TenantEmail,
    string TenantName,
    decimal TotalAmount,
    string ApartmentUnit
) : IntegrationEvent;

public record ContractApprovedIntegrationEvent(
    int ContractId,
    int TenantId,
    string TenantEmail,
    string TenantName,
    int ApprovedBy,
    string ApprovedByName,
    decimal TotalAmount,
    int NumberOfInstallments,
    DateTime StartDate,
    DateTime EndDate
) : IntegrationEvent;

public record ContractRejectedIntegrationEvent(
    int ContractId,
    int TenantId,
    string TenantEmail,
    string TenantName,
    int RejectedBy,
    string RejectedByName,
    string Reason
) : IntegrationEvent;

public record InstallmentsGeneratedIntegrationEvent(
    int ContractId,
    int TenantId,
    string TenantEmail,
    List<InstallmentInfo> Installments
) : IntegrationEvent;

public record InstallmentInfo(
    int InstallmentId,
    decimal Amount,
    DateTime DueDate,
    int InstallmentNumber
);


