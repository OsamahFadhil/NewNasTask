using Insight.Invoicing.Application.DTOs;
using MediatR;

namespace Insight.Invoicing.Application.Queries.Contracts;

public record GetUserContractsQuery(
    int TenantId,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<(IEnumerable<ContractSummaryDto> Contracts, int TotalCount)>;

