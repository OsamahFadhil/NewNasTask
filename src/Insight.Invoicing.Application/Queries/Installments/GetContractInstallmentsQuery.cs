using Insight.Invoicing.Application.DTOs;
using MediatR;

namespace Insight.Invoicing.Application.Queries.Installments;

public record GetContractInstallmentsQuery(
    int ContractId,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<(IEnumerable<InstallmentDto> Installments, int TotalCount)>;

