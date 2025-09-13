using MediatR;

namespace Insight.Invoicing.Application.Commands.Contracts;

public record RejectContractCommand(
    int ContractId,
    int RejectedBy,
    string Reason
) : IRequest<bool>;

