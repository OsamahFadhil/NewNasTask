using MediatR;

namespace Insight.Invoicing.Application.Commands.Contracts;

public record SubmitContractCommand(int ContractId) : IRequest<bool>;

