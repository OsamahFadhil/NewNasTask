using Insight.Invoicing.Domain.Enums;
using Insight.Invoicing.Domain.ValueObjects;
using Insight.Invoicing.Shared.Common;

namespace Insight.Invoicing.Domain.Entities;

public class User : AggregateRoot
{
    public string UserName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string PhoneNumber { get; private set; } = string.Empty;

    public Address? Address { get; private set; }

    public UserRole Role { get; private set; }

    public bool IsActive { get; private set; } = true;

    private User() { }

    public User(string userName, string email, string firstName, string lastName, string phoneNumber, UserRole role, Address? address = null)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("User name cannot be null or empty", nameof(userName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be null or empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be null or empty", nameof(lastName));

        UserName = userName.Trim();
        Email = email.Trim().ToLowerInvariant();
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        PhoneNumber = phoneNumber?.Trim() ?? string.Empty;
        Address = address;
        Role = role;
    }

    public string GetFullName() => $"{FirstName} {LastName}";

    public void UpdateProfile(string firstName, string lastName, string phoneNumber, Address? address = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be null or empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be null or empty", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        PhoneNumber = phoneNumber?.Trim() ?? string.Empty;
        Address = address;

        UpdateTimestamp();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateTimestamp();
    }

    public void Activate()
    {
        IsActive = true;
        UpdateTimestamp();
    }

    public bool IsTenant() => Role == UserRole.Tenant;

    public bool IsAdministrator() => Role == UserRole.Administrator;
}

