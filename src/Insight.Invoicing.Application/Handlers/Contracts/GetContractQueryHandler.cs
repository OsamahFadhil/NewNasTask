using Insight.Invoicing.Application.DTOs;
using Insight.Invoicing.Application.Queries.Contracts;
using Insight.Invoicing.Domain.Repositories;
using MediatR;

namespace Insight.Invoicing.Application.Handlers.Contracts;

public class GetContractQueryHandler : IRequestHandler<GetContractQuery, ContractDto?>
{
    private readonly IContractRepository _contractRepository;

    public GetContractQueryHandler(IContractRepository contractRepository)
    {
        _contractRepository = contractRepository;
    }

    public async Task<ContractDto?> Handle(GetContractQuery request, CancellationToken cancellationToken)
    {
        var contract = await _contractRepository.GetByIdAsync(request.ContractId, cancellationToken);

        if (contract == null)
        {
            return null;
        }

        var contractDto = new ContractDto(
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
        );

        return contractDto;
    }
}
