using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Insight.Invoicing.Application.DTOs;
using Insight.Invoicing.Application.Queries.Authentication;
using Insight.Invoicing.Domain.Entities;
using Insight.Invoicing.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Insight.Invoicing.Infrastructure.Handlers.Authentication;

public class ValidateTokenQueryHandler : IRequestHandler<ValidateTokenQuery, UserDto?>
{
    private readonly IRepository<User> _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ValidateTokenQueryHandler> _logger;

    public ValidateTokenQueryHandler(
        IRepository<User> userRepository,
        IConfiguration configuration,
        ILogger<ValidateTokenQueryHandler> logger)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<UserDto?> Handle(ValidateTokenQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["JwtSettings:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["JwtSettings:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(request.Token, validationParameters, out _);
            var userIdClaim = principal.FindFirst("UserId")?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return null;
            }

            var domainUser = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (domainUser == null || !domainUser.IsActive)
            {
                return null;
            }

            return MapToUserDto(domainUser);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return null;
        }
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto(
            user.Id,
            user.UserName,
            user.Email,
            user.FirstName,
            user.LastName,
            user.GetFullName(),
            user.PhoneNumber,
            user.Role,
            user.IsActive,
            user.CreatedAt,
            user.UpdatedAt
        );
    }
}


