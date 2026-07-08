using System.Text.RegularExpressions;

namespace CondoSync.Core.ValueObjects;

public class Email : IEquatable<Email>
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email não pode ser vazio", nameof(value));

        if (!IsValidEmail(value))
            throw new ArgumentException($"Email inválido: {value}", nameof(value));

        Value = value.ToLowerInvariant().Trim();
    }

    private static bool IsValidEmail(string email)
    {
        // RFC 5322 simplificado
        var regex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
        return regex.IsMatch(email);
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj) => obj is Email other && Equals(other);

    public bool Equals(Email? other) => other is not null && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(Email left, Email right) => left.Equals(right);

    public static bool operator !=(Email left, Email right) => !left.Equals(right);

    public static implicit operator string(Email email) => email.Value;

    public static explicit operator Email(string value) => new(value);
}