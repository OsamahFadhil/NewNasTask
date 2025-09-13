using Insight.Invoicing.Application.DTOs;
using MediatR;

namespace Insight.Invoicing.Application.Commands.Contracts;

public record CreateContractCommand(
    int TenantId,
    string ApartmentUnit,
    decimal TotalAmount,
    decimal InitialPayment,
    int NumberOfInstallments,
    DateTime StartDate,
    DateTime EndDate
) : IRequest<ContractDto>;

