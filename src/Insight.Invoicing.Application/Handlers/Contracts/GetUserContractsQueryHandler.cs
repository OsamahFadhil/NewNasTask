using Insight.Invoicing.Application.DTOs;
using Insight.Invoicing.Application.Queries.Contracts;
using Insight.Invoicing.Domain.Repositories;
using MediatR;

namespace Insight.Invoicing.Application.Handlers.Contracts;

public class GetUserContractsQueryHandler : IRequestHandler<GetUserContractsQuery, (IEnumerable<ContractSummaryDto> Contracts, int TotalCount)>
{
    private readonly IContractRepository _contractRepository;

    public GetUserContractsQueryHandler(IContractRepository contractRepository)
    {
        _contractRepository = contractRepository;
    }

    public async Task<(IEnumerable<ContractSummaryDto> Contracts, int TotalCount)> Handle(
        GetUserContractsQuery request,
        CancellationToken cancellationToken)
    {
        var (contracts, totalCount) = await _contractRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            contract => contract.TenantId == request.TenantId,
            contract => contract.CreatedAt, // Order by creation date
            cancellationToken);

        var contractDtos = contracts.Select(contract => new ContractSummaryDto(
            contract.Id,
            contract.ApartmentUnit,
            contract.TotalAmount.Amount,
            contract.InitialPayment.Amount, // For now, using initial payment as paid amount
            contract.TotalAmount.Amount - contract.InitialPayment.Amount, // Outstanding amount
            contract.Status,
            contract.NumberOfInstallments,
            0, // Paid installments - would need to calculate from actual payments
            0, // Overdue installments - would need to calculate from due dates
            contract.StartDate,
            contract.EndDate
        ));

        return (contractDtos, totalCount);
    }
}
