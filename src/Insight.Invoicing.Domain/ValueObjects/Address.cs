namespace Insight.Invoicing.Domain.ValueObjects;

public record Address
{
    public string Street { get; init; }

    public string City { get; init; }

    public string State { get; init; }

    public string PostalCode { get; init; }

    public Address(string street, string city, string state, string postalCode)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be null or empty", nameof(street));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be null or empty", nameof(city));

        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State cannot be null or empty", nameof(state));

        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code cannot be null or empty", nameof(postalCode));

        Street = street.Trim();
        City = city.Trim();
        State = state.Trim();
        PostalCode = postalCode.Trim();
    }

    public string GetFullAddress()
    {
        return $"{Street}, {City}, {State} {PostalCode}";
    }

    public override string ToString() => GetFullAddress();
}

