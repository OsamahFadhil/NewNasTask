using Insight.Invoicing.Application.DTOs;
using MediatR;

namespace Insight.Invoicing.Application.Queries.Installments;

public record GetOverdueInstallmentsQuery(
    int PageNumber = 1,
    int PageSize = 50,
    int? MinOverdueDays = null
) : IRequest<(IEnumerable<InstallmentDto> Installments, int TotalCount)>;

