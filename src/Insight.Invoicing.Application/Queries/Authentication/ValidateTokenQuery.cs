using Insight.Invoicing.Application.DTOs;
using MediatR;

namespace Insight.Invoicing.Application.Queries.Authentication;

public record ValidateTokenQuery(
    string Token
) : IRequest<UserDto?>;

