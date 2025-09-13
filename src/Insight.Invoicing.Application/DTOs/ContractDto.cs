using Insight.Invoicing.Domain.Enums;

namespace Insight.Invoicing.Application.DTOs;

public record ContractDto(
    int Id,
    int TenantId,
    string TenantName,
    string ApartmentUnit,
    decimal TotalAmount,
    decimal InitialPayment,
    int NumberOfInstallments,
    DateTime StartDate,
    DateTime EndDate,
    ContractStatus Status,
    string? Comments,
    int? LastUpdatedBy,
    string? LastUpdatedByName,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateContractDto(
    int TenantId,
    string ApartmentUnit,
    decimal TotalAmount,
    decimal InitialPayment,
    int NumberOfInstallments,
    DateTime StartDate,
    DateTime EndDate
);

public record UpdateContractDto(
    string ApartmentUnit,
    decimal TotalAmount,
    decimal InitialPayment,
    int NumberOfInstallments,
    DateTime StartDate,
    DateTime EndDate
);

public record ContractActionDto(
    string? Comments
);

public record ContractSummaryDto(
    int Id,
    string ApartmentUnit,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal OutstandingAmount,
    ContractStatus Status,
    int TotalInstallments,
    int PaidInstallments,
    int OverdueInstallments,
    DateTime StartDate,
    DateTime EndDate
);

