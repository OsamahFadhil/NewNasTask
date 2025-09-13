using Insight.Invoicing.Application.Commands.Authentication;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Insight.Invoicing.Infrastructure.Handlers.Authentication;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly UserManager<IdentityUser<int>> _userManager;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(
        UserManager<IdentityUser<int>> userManager,
        ILogger<ChangePasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var identityUser = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (identityUser == null)
            {
                _logger.LogWarning("Password change failed: Identity user not found: {UserId}", request.UserId);
                return false;
            }

            var result = await _userManager.ChangePasswordAsync(identityUser, request.CurrentPassword, request.NewPassword);
            if (result.Succeeded)
            {
                _logger.LogInformation("Password changed successfully for user: {UserId}", request.UserId);
                return true;
            }

            _logger.LogWarning("Password change failed for user {UserId}: {Errors}",
                request.UserId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password change for user: {UserId}", request.UserId);
            return false;
        }
    }
}

