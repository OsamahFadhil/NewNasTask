using Insight.Invoicing.Shared.Common;

namespace Insight.Invoicing.Domain.Events;

public record ContractSubmittedEvent(
    int ContractId,
    int TenantId,
    decimal TotalAmount
) : DomainEvent;

public record ContractApprovedEvent(
    int ContractId,
    int TenantId,
    int ApprovedBy,
    decimal TotalAmount,
    int NumberOfInstallments
) : DomainEvent;

public record ContractRejectedEvent(
    int ContractId,
    int TenantId,
    int RejectedBy,
    string Reason
) : DomainEvent;

public record ContractUpdateRequestedEvent(
    int ContractId,
    int TenantId,
    int RequestedBy,
    string Reason
) : DomainEvent;

