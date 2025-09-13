using MediatR;

namespace Insight.Invoicing.Application.Commands.Contracts;

public record ApproveContractCommand(
    int ContractId,
    int ApprovedBy,
    string? Comments = null
) : IRequest<bool>;

