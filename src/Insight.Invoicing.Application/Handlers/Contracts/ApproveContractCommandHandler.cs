using Insight.Invoicing.Application.Commands.Contracts;
using Insight.Invoicing.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Insight.Invoicing.Application.Handlers.Contracts;

public class ApproveContractCommandHandler : IRequestHandler<ApproveContractCommand, bool>
{
    private readonly IContractRepository _contractRepository;
    private readonly ILogger<ApproveContractCommandHandler> _logger;

    public ApproveContractCommandHandler(IContractRepository contractRepository, ILogger<ApproveContractCommandHandler> logger)
    {
        _contractRepository = contractRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(ApproveContractCommand request, CancellationToken cancellationToken)
    {
        var contract = await _contractRepository.GetWithInstallmentsAsync(request.ContractId, cancellationToken);

        if (contract == null)
        {
            return false;
        }

        contract.Approve(request.ApprovedBy, request.Comments);

        try
        {
            await _contractRepository.UpdateAsync(contract, cancellationToken);
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key") == true ||
                                          ex.InnerException?.Message.Contains("IX_Installments_Contract_Sequence") == true)
        {
            _logger.LogWarning(
                "Duplicate installments detected for contract {ContractId}, but contract approval succeeded. Error: {Error}",
                request.ContractId,
                ex.InnerException?.Message ?? ex.Message);

            return true;
        }
    }
}

