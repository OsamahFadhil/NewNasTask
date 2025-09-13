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

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthenticationResultDto>
{
    private readonly UserManager<IdentityUser<int>> _userManager;
    private readonly SignInManager<IdentityUser<int>> _signInManager;
    private readonly IRepository<User> _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        UserManager<IdentityUser<int>> userManager,
        SignInManager<IdentityUser<int>> signInManager,
        IRepository<User> userRepository,
        IConfiguration configuration,
        ILogger<LoginCommandHandler> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthenticationResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Attempting login for user: {UserName}", request.UserName);

            var domainUser = await _userRepository.GetSingleAsync(
                u => u.UserName == request.UserName && u.IsActive,
                cancellationToken);

            if (domainUser == null)
            {
                _logger.LogWarning("Login failed: User not found or inactive: {UserName}", request.UserName);
                return new AuthenticationResultDto(false, null, null, null, "Invalid username or password");
            }

            var identityUser = await _userManager.FindByNameAsync(request.UserName);
            if (identityUser == null)
            {
                _logger.LogWarning("Login failed: Identity user not found: {UserName}", request.UserName);
                return new AuthenticationResultDto(false, null, null, null, "Invalid username or password");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(identityUser, request.Password, false);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Login failed: Invalid password for user: {UserName}", request.UserName);
                return new AuthenticationResultDto(false, null, null, null, "Invalid username or password");
            }

            var token = GenerateJwtToken(domainUser, identityUser);
            var expiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("JwtSettings:ExpirationInMinutes", 60));

            var userDto = MapToUserDto(domainUser);

            _logger.LogInformation("Login successful for user: {UserName}", request.UserName);
            return new AuthenticationResultDto(true, token, expiresAt, userDto, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {UserName}", request.UserName);
            return new AuthenticationResultDto(false, null, null, null, "An error occurred during login");
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


