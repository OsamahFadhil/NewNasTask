using Insight.Invoicing.Application.DTOs;
using Insight.Invoicing.Application.Queries.Contracts;
using Insight.Invoicing.Domain.Repositories;
using MediatR;

namespace Insight.Invoicing.Application.Handlers.Contracts;

public class GetPendingContractsQueryHandler : IRequestHandler<GetPendingContractsQuery, (IEnumerable<ContractDto> Contracts, int TotalCount)>
{
    private readonly IContractRepository _contractRepository;

    public GetPendingContractsQueryHandler(IContractRepository contractRepository)
    {
        _contractRepository = contractRepository;
    }

    public async Task<(IEnumerable<ContractDto> Contracts, int TotalCount)> Handle(
        GetPendingContractsQuery request,
        CancellationToken cancellationToken)
    {
        var (contracts, totalCount) = await _contractRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            contract => contract.Status == Domain.Enums.ContractStatus.Submitted,
            contract => contract.CreatedAt, // Order by creation date
            cancellationToken);

        var contractDtos = contracts.Select(contract => new ContractDto(
            contract.Id,
            contract.TenantId,
            "Tenant Name", // TODO: Get actual tenant name from user service
            contract.ApartmentUnit,
            contract.TotalAmount.Amount,
            contract.InitialPayment.Amount,
            contract.NumberOfInstallments,
            contract.StartDate,
            contract.EndDate,
            contract.Status,
            contract.Comments,
            contract.LastUpdatedBy,
            "Updated By Name", // TODO: Get actual user name from user service
            contract.CreatedAt,
            contract.UpdatedAt
        ));

        return (contractDtos, totalCount);
    }
}
