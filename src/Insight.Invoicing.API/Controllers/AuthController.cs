using Insight.Invoicing.Application.Commands.Authentication;
using Insight.Invoicing.Application.DTOs;
using Insight.Invoicing.Application.Queries.Authentication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Insight.Invoicing.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

 
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthenticationResultDto>> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Login attempt for user: {UserName}", loginDto.UserName);

        var command = new LoginCommand(loginDto.UserName, loginDto.Password);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Login failed for user: {UserName}", loginDto.UserName);
            return Unauthorized(new { message = result.ErrorMessage });
        }

        _logger.LogInformation("Login successful for user: {UserName}", loginDto.UserName);
        return Ok(result);
    }

    
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthenticationResultDto>> Register([FromBody] CreateUserDto createUserDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Registration attempt for user: {UserName}", createUserDto.UserName);

        var command = new RegisterCommand(
            createUserDto.UserName,
            createUserDto.Email,
            createUserDto.FirstName,
            createUserDto.LastName,
            createUserDto.PhoneNumber,
            createUserDto.Role,
            createUserDto.Password);

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Registration failed for user: {UserName}", createUserDto.UserName);
            return BadRequest(new { message = result.ErrorMessage });
        }

        _logger.LogInformation("Registration successful for user: {UserName}", createUserDto.UserName);
        return CreatedAtAction(nameof(GetCurrentUser), result);
    }


    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return BadRequest("Invalid user ID");
        }

        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader != null && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            var query = new ValidateTokenQuery(token);
            var user = await _mediator.Send(query);

            if (user != null)
            {
                return Ok(user);
            }
        }

        return NotFound("User not found");
    }


    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return BadRequest("Invalid user ID");
        }

        var command = new ChangePasswordCommand(userId, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
        var result = await _mediator.Send(command);

        if (!result)
        {
            return BadRequest(new { message = "Password change failed. Please check your current password." });
        }

        return Ok(new { message = "Password changed successfully" });
    }


    [HttpPost("validate-token")]
    [AllowAnonymous]
    public async Task<ActionResult> ValidateToken([FromBody] ValidateTokenDto tokenDto)
    {
        if (string.IsNullOrWhiteSpace(tokenDto.Token))
        {
            return BadRequest(new { message = "Token is required" });
        }

        var query = new ValidateTokenQuery(tokenDto.Token);
        var user = await _mediator.Send(query);

        if (user == null)
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        return Ok(new { valid = true, user });
    }

    
}


public record ChangePasswordDto(
    [Required] string CurrentPassword,
    [Required][MinLength(8)] string NewPassword
);


public record ValidateTokenDto(
    [Required] string Token
);
