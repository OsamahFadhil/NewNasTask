using Insight.Invoicing.Application.DTOs;
using Insight.Invoicing.Application.Queries.Installments;
using Insight.Invoicing.Domain.Repositories;
using MediatR;

namespace Insight.Invoicing.Application.Handlers.Installments;

public class GetOverdueInstallmentsQueryHandler : IRequestHandler<GetOverdueInstallmentsQuery, (IEnumerable<InstallmentDto> Installments, int TotalCount)>
{
    private readonly IContractRepository _contractRepository;

    public GetOverdueInstallmentsQueryHandler(IContractRepository contractRepository)
    {
        _contractRepository = contractRepository;
    }

    public async Task<(IEnumerable<InstallmentDto> Installments, int TotalCount)> Handle(GetOverdueInstallmentsQuery request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        return (Enumerable.Empty<InstallmentDto>(), 0);
    }
}
