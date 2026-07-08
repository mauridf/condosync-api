namespace CondoSync.Core.ValueObjects;

public class Money : IEquatable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency = "BRL")
    {
        if (amount < 0)
            throw new ArgumentException("Valor monetário não pode ser negativo", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Moeda não pode ser vazia", nameof(currency));

        Amount = Math.Round(amount, 2);
        Currency = currency.ToUpperInvariant();
    }

    public Money Add(Money other)
    {
        ValidateSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        ValidateSameCurrency(other);
        var result = Amount - other.Amount;
        if (result < 0)
            throw new InvalidOperationException("Resultado da subtração não pode ser negativo");
        return new Money(result, Currency);
    }

    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new ArgumentException("Fator de multiplicação não pode ser negativo", nameof(factor));
        return new Money(Amount * factor, Currency);
    }

    public Money Percentage(decimal percentage)
    {
        return new Money(Amount * (percentage / 100), Currency);
    }

    private void ValidateSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Moedas diferentes: {Currency} vs {other.Currency}");
    }

    public override string ToString() => $"{Currency} {Amount:N2}";

    public override bool Equals(object? obj) => obj is Money other && Equals(other);

    public bool Equals(Money? other) =>
        other is not null && Amount == other.Amount && Currency == other.Currency;

    public override int GetHashCode() => HashCode.Combine(Amount, Currency);

    public static bool operator ==(Money left, Money right) => left.Equals(right);

    public static bool operator !=(Money left, Money right) => !left.Equals(right);

    public static bool operator >(Money left, Money right) =>
        left.Currency == right.Currency && left.Amount > right.Amount;

    public static bool operator <(Money left, Money right) =>
        left.Currency == right.Currency && left.Amount < right.Amount;

    public static Money Zero(string currency = "BRL") => new(0, currency);
}