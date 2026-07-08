using System.Text.RegularExpressions;

namespace CondoSync.Core.ValueObjects;

public class DocumentNumber : IEquatable<DocumentNumber>
{
    public string Value { get; }
    public DocumentType Type { get; }

    public DocumentNumber(string value, DocumentType type = DocumentType.CPF)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Documento não pode ser vazio", nameof(value));

        var digitsOnly = Regex.Replace(value, @"[^\d]", "");
        Type = type;

        Value = type switch
        {
            DocumentType.CPF => ValidateCPF(digitsOnly),
            DocumentType.CNPJ => ValidateCNPJ(digitsOnly),
            _ => digitsOnly
        };
    }

    private static string ValidateCPF(string cpf)
    {
        if (cpf.Length != 11)
            throw new ArgumentException("CPF deve ter 11 dígitos", nameof(cpf));

        // Validação simples de dígitos verificadores
        if (!IsValidCPF(cpf))
            throw new ArgumentException("CPF inválido", nameof(cpf));

        return $"{cpf[..3]}.{cpf[3..6]}.{cpf[6..9]}-{cpf[9..]}";
    }

    private static string ValidateCNPJ(string cnpj)
    {
        if (cnpj.Length != 14)
            throw new ArgumentException("CNPJ deve ter 14 dígitos", nameof(cnpj));

        if (!IsValidCNPJ(cnpj))
            throw new ArgumentException("CNPJ inválido", nameof(cnpj));

        return $"{cnpj[..2]}.{cnpj[2..5]}.{cnpj[5..8]}/{cnpj[8..12]}-{cnpj[12..]}";
    }

    private static bool IsValidCPF(string cpf)
    {
        // Implementação simplificada - em produção usar algoritmo completo
        if (cpf.Distinct().Count() == 1) return false; // Todos dígitos iguais

        int[] multiplier1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplier2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        var tempCpf = cpf[..9];
        var sum = 0;

        for (int i = 0; i < 9; i++)
            sum += int.Parse(tempCpf[i].ToString()) * multiplier1[i];

        var remainder = sum % 11;
        remainder = remainder < 2 ? 0 : 11 - remainder;
        var digit = remainder.ToString();
        tempCpf += digit;

        sum = 0;
        for (int i = 0; i < 10; i++)
            sum += int.Parse(tempCpf[i].ToString()) * multiplier2[i];

        remainder = sum % 11;
        remainder = remainder < 2 ? 0 : 11 - remainder;
        digit += remainder.ToString();

        return cpf.EndsWith(digit);
    }

    private static bool IsValidCNPJ(string cnpj)
    {
        // Implementação simplificada - em produção usar algoritmo completo
        return cnpj.Length == 14;
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj) => obj is DocumentNumber other && Equals(other);

    public bool Equals(DocumentNumber? other) =>
        other is not null && Value == other.Value && Type == other.Type;

    public override int GetHashCode() => HashCode.Combine(Value, Type);

    public static bool operator ==(DocumentNumber left, DocumentNumber right) => left.Equals(right);

    public static bool operator !=(DocumentNumber left, DocumentNumber right) => !left.Equals(right);

    public static implicit operator string(DocumentNumber doc) => doc.Value;

    public static explicit operator DocumentNumber(string value) => new(value);
}

public enum DocumentType
{
    CPF,
    CNPJ,
    RG,
    Passport,
    Other
}