using Insight.Invoicing.Application.DTOs;
using MediatR;

namespace Insight.Invoicing.Application.Queries.Contracts;

public record GetContractQuery(int ContractId) : IRequest<ContractDto?>;

