using Insight.Invoicing.Application.Commands.Contracts;
using Insight.Invoicing.Domain.Repositories;
using MediatR;

namespace Insight.Invoicing.Application.Handlers.Contracts;

public class SubmitContractCommandHandler : IRequestHandler<SubmitContractCommand, bool>
{
    private readonly IContractRepository _contractRepository;

    public SubmitContractCommandHandler(IContractRepository contractRepository)
    {
        _contractRepository = contractRepository;
    }

    public async Task<bool> Handle(SubmitContractCommand request, CancellationToken cancellationToken)
    {
        var contract = await _contractRepository.GetByIdAsync(request.ContractId, cancellationToken);

        if (contract == null)
        {
            return false;
        }

        contract.Submit();
        await _contractRepository.UpdateAsync(contract, cancellationToken);

        return true;
    }
}

