using System.Text.RegularExpressions;

namespace CondoSync.Core.ValueObjects;

public class PhoneNumber : IEquatable<PhoneNumber>
{
    public string Value { get; }

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Telefone não pode ser vazio", nameof(value));

        // Remove tudo que não for dígito
        var digitsOnly = Regex.Replace(value, @"[^\d]", "");

        if (digitsOnly.Length < 10 || digitsOnly.Length > 11)
            throw new ArgumentException($"Telefone deve ter 10 ou 11 dígitos. Fornecido: {digitsOnly.Length}", nameof(value));

        Value = digitsOnly;
    }

    public string Format()
    {
        if (Value.Length == 11)
            return $"({Value[..2]}) {Value[2..7]}-{Value[7..]}";
        else
            return $"({Value[..2]}) {Value[2..6]}-{Value[6..]}";
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj) => obj is PhoneNumber other && Equals(other);

    public bool Equals(PhoneNumber? other) => other is not null && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(PhoneNumber left, PhoneNumber right) => left.Equals(right);

    public static bool operator !=(PhoneNumber left, PhoneNumber right) => !left.Equals(right);

    public static implicit operator string(PhoneNumber phone) => phone.Value;

    public static explicit operator PhoneNumber(string value) => new(value);
}