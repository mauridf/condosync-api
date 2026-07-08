namespace CondoSync.Core.ValueObjects;

public class Address : IEquatable<Address>
{
    public string Street { get; }
    public string? Number { get; }
    public string? Complement { get; }
    public string? Neighborhood { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }

    public Address(string street, string city, string state, string zipCode,
        string? number = null, string? complement = null, string? neighborhood = null)
    {
        Street = street ?? throw new ArgumentNullException(nameof(street));
        City = city ?? throw new ArgumentNullException(nameof(city));
        State = ValidateState(state);
        ZipCode = ValidateZipCode(zipCode);
        Number = number;
        Complement = complement;
        Neighborhood = neighborhood;
    }

    private static string ValidateState(string state)
    {
        if (state.Length != 2)
            throw new ArgumentException("Estado deve ter 2 caracteres (UF)", nameof(state));
        return state.ToUpperInvariant();
    }

    private static string ValidateZipCode(string zipCode)
    {
        var digitsOnly = System.Text.RegularExpressions.Regex.Replace(zipCode, @"[^\d]", "");
        if (digitsOnly.Length != 8)
            throw new ArgumentException("CEP deve ter 8 dígitos", nameof(zipCode));
        return $"{digitsOnly[..5]}-{digitsOnly[5..]}";
    }

    public override string ToString() =>
        $"{Street}, {Number} - {Neighborhood}, {City}/{State} - CEP: {ZipCode}";

    public override bool Equals(object? obj) => obj is Address other && Equals(other);

    public bool Equals(Address? other) =>
        other is not null &&
        Street == other.Street &&
        City == other.City &&
        State == other.State &&
        ZipCode == other.ZipCode;

    public override int GetHashCode() =>
        HashCode.Combine(Street, City, State, ZipCode);

    public static bool operator ==(Address left, Address right) => left.Equals(right);

    public static bool operator !=(Address left, Address right) => !left.Equals(right);
}