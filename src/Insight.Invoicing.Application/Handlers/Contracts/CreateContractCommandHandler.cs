using Insight.Invoicing.Application.Commands.Contracts;
using Insight.Invoicing.Application.DTOs;
using Insight.Invoicing.Domain.Entities;
using Insight.Invoicing.Domain.Repositories;
using MediatR;
using Insight.Invoicing.Domain.ValueObjects;

namespace Insight.Invoicing.Application.Handlers.Contracts;

public class CreateContractCommandHandler : IRequestHandler<CreateContractCommand, ContractDto>
{
    private readonly IContractRepository _contractRepository;

    public CreateContractCommandHandler(IContractRepository contractRepository)
    {
        _contractRepository = contractRepository;
    }

    public async Task<ContractDto> Handle(CreateContractCommand request, CancellationToken cancellationToken)
    {
        var contract = new Contract(
            request.TenantId,
            request.ApartmentUnit,
            Money.Usd(request.TotalAmount),
            Money.Usd(request.InitialPayment),
            request.NumberOfInstallments,
            request.StartDate,
            request.EndDate);

        var createdContract = await _contractRepository.AddAsync(contract, cancellationToken);

        return new ContractDto(
            createdContract.Id,
            createdContract.TenantId,
            "", // Will be populated by the repository with joins
            createdContract.ApartmentUnit,
            createdContract.TotalAmount.Amount,
            createdContract.InitialPayment.Amount,
            createdContract.NumberOfInstallments,
            createdContract.StartDate,
            createdContract.EndDate,
            createdContract.Status,
            createdContract.Comments,
            createdContract.LastUpdatedBy,
            "", // Will be populated by the repository with joins
            createdContract.CreatedAt,
            createdContract.UpdatedAt);
    }
}

