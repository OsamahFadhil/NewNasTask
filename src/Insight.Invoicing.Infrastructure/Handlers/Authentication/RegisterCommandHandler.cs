using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Insight.Invoicing.Application.Commands.Authentication;
using Insight.Invoicing.Application.DTOs;
using Insight.Invoicing.Domain.Entities;
using Insight.Invoicing.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Insight.Invoicing.Infrastructure.Handlers.Authentication;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthenticationResultDto>
{
    private readonly UserManager<IdentityUser<int>> _userManager;
    private readonly IRepository<User> _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        UserManager<IdentityUser<int>> userManager,
        IRepository<User> userRepository,
        IConfiguration configuration,
        ILogger<RegisterCommandHandler> logger)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthenticationResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Attempting registration for user: {UserName}", request.UserName);

            var existingUser = await _userRepository.GetSingleAsync(
                u => u.UserName == request.UserName || u.Email == request.Email,
                cancellationToken);

            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed: User already exists: {UserName}", request.UserName);
                return new AuthenticationResultDto(false, null, null, null, "User already exists");
            }

            var identityUser = new IdentityUser<int>
            {
                UserName = request.UserName,
                Email = request.Email,
                EmailConfirmed = true // For simplicity, auto-confirm emails
            };

            var identityResult = await _userManager.CreateAsync(identityUser, request.Password);
            if (!identityResult.Succeeded)
            {
                var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                _logger.LogWarning("Registration failed: Identity creation failed for {UserName}: {Errors}",
                    request.UserName, errors);
                return new AuthenticationResultDto(false, null, null, null, $"Registration failed: {errors}");
            }

            var domainUser = new User(
                request.UserName,
                request.Email,
                request.FirstName,
                request.LastName,
                request.PhoneNumber,
                request.Role);

            domainUser.Id = identityUser.Id;

            await _userRepository.AddAsync(domainUser, cancellationToken);

            var token = GenerateJwtToken(domainUser, identityUser);
            var expiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("JwtSettings:ExpirationInMinutes", 60));

            var userDto = MapToUserDto(domainUser);

            _logger.LogInformation("Registration successful for user: {UserName}", request.UserName);
            return new AuthenticationResultDto(true, token, expiresAt, userDto, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user: {UserName}", request.UserName);
            return new AuthenticationResultDto(false, null, null, null, "An error occurred during registration");
        }
    }

    private string GenerateJwtToken(User domainUser, IdentityUser<int> identityUser)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, identityUser.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, domainUser.Email),
            new("UserId", domainUser.Id.ToString()),
            new(ClaimTypes.Name, domainUser.UserName),
            new(ClaimTypes.Email, domainUser.Email),
            new(ClaimTypes.Role, domainUser.Role.ToString()),
            new("FullName", domainUser.GetFullName())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("JwtSettings:ExpirationInMinutes", 60)),
            Issuer = _configuration["JwtSettings:Issuer"],
            Audience = _configuration["JwtSettings:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
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

