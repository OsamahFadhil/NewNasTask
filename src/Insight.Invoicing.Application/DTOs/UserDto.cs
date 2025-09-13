using Insight.Invoicing.Domain.Enums;

namespace Insight.Invoicing.Application.DTOs;

public record UserDto(
    int Id,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string PhoneNumber,
    UserRole Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateUserDto(
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    UserRole Role,
    string Password
);

public record UpdateUserProfileDto(
    string FirstName,
    string LastName,
    string PhoneNumber
);

public record LoginDto(
    string UserName,
    string Password
);

public record AuthenticationResultDto(
    bool IsSuccess,
    string? Token,
    DateTime? ExpiresAt,
    UserDto? User,
    string? ErrorMessage
);

