using MediatR;

namespace Insight.Invoicing.Application.Commands.Authentication;

public record ChangePasswordCommand(
    int UserId,
    string CurrentPassword,
    string NewPassword
) : IRequest<bool>;

