using Insight.Invoicing.Application.DTOs;
using MediatR;

namespace Insight.Invoicing.Application.Commands.Authentication;

public record LoginCommand(
    string UserName,
    string Password
) : IRequest<AuthenticationResultDto>;

