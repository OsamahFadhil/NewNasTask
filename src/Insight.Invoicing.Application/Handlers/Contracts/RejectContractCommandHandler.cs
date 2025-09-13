using Insight.Invoicing.Application.Commands.Contracts;
using Insight.Invoicing.Domain.Repositories;
using MediatR;

namespace Insight.Invoicing.Application.Handlers.Contracts;

public class RejectContractCommandHandler : IRequestHandler<RejectContractCommand, bool>
{
    private readonly IContractRepository _contractRepository;

    public RejectContractCommandHandler(IContractRepository contractRepository)
    {
        _contractRepository = contractRepository;
    }

    public async Task<bool> Handle(RejectContractCommand request, CancellationToken cancellationToken)
    {
        var contract = await _contractRepository.GetByIdAsync(request.ContractId, cancellationToken);

        if (contract == null)
        {
            return false;
        }

        contract.Reject(request.RejectedBy, request.Reason);
        await _contractRepository.UpdateAsync(contract, cancellationToken);

        return true;
    }
}
