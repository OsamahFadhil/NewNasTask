using Insight.Invoicing.Application.DTOs;
using Insight.Invoicing.Domain.Enums;
using MediatR;

namespace Insight.Invoicing.Application.Commands.Authentication;

public record RegisterCommand(
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    UserRole Role,
    string Password
) : IRequest<AuthenticationResultDto>;

