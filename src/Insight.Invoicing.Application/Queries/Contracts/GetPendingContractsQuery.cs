using Insight.Invoicing.Application.DTOs;
using MediatR;

namespace Insight.Invoicing.Application.Queries.Contracts;

public record GetPendingContractsQuery(
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<(IEnumerable<ContractDto> Contracts, int TotalCount)>;

