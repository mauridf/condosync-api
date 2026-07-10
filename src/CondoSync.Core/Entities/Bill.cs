using CondoSync.Core.Enums;
using CondoSync.Core.Events;
using CondoSync.Core.Exceptions;
using CondoSync.Core.ValueObjects;

namespace CondoSync.Core.Entities;

public class Bill : AggregateRoot<Guid>, ITenantEntity
{
    public Guid CondominiumId { get; private set; }
    public Guid UnitId { get; private set; }

    // Identificação
    public string? BillNumber { get; private set; }
    public string Description { get; private set; } = default!;
    public string ReferenceMonth { get; private set; } = default!;

    // Valores
    public decimal BaseAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public string? DiscountType { get; private set; }
    public decimal FineAmount { get; private set; }
    public decimal InterestAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal? Balance { get; private set; }

    // Datas
    public DateOnly IssueDate { get; private set; }
    public DateOnly DueDate { get; private set; }
    public DateOnly? FineStartDate { get; private set; }

    // Configuração de multa
    public decimal LateFeePercentage { get; private set; }
    public decimal LateInterestDaily { get; private set; }
    public int MaxInterestMonths { get; private set; }

    // Status
    public BillStatus Status { get; private set; }

    // Pagamento
    public DateOnly? PaymentDate { get; private set; }
    public decimal? PaymentAmount { get; private set; }
    public string? PaymentMethod { get; private set; }
    public string? TransactionId { get; private set; }
    public Guid? PaidBy { get; private set; }

    // Parcelamento
    public int InstallmentNumber { get; private set; }
    public int TotalInstallments { get; private set; }
    public Guid? ParentBillId { get; private set; }

    // Boleto/PIX
    public string? BoletoUrl { get; private set; }
    public string? BoletoCode { get; private set; }
    public string? PixCode { get; private set; }
    public string? PixQrCodeUrl { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Bill() { }

    public static Bill Create(
        Guid condominiumId,
        Guid unitId,
        string description,
        string referenceMonth,
        decimal baseAmount,
        DateOnly dueDate,
        decimal lateFeePercentage = 2.00m,
        decimal lateInterestDaily = 0.033m,
        int maxInterestMonths = 12,
        int installmentNumber = 1,
        int totalInstallments = 1,
        Guid? parentBillId = null)
    {
        if (baseAmount <= 0)
            throw new DomainException("Valor base da fatura deve ser maior que zero");

        if (string.IsNullOrWhiteSpace(referenceMonth))
            throw new DomainException("Mês de referência não pode ser vazio");

        var bill = new Bill
        {
            Id = Guid.NewGuid(),
            CondominiumId = condominiumId,
            UnitId = unitId,
            Description = description,
            ReferenceMonth = referenceMonth,
            BaseAmount = baseAmount,
            TotalAmount = baseAmount,
            DueDate = dueDate,
            IssueDate = DateOnly.FromDateTime(DateTime.UtcNow),
            LateFeePercentage = lateFeePercentage,
            LateInterestDaily = lateInterestDaily,
            MaxInterestMonths = maxInterestMonths,
            Status = BillStatus.Pending,
            InstallmentNumber = installmentNumber,
            TotalInstallments = totalInstallments,
            ParentBillId = parentBillId,
            BillNumber = GenerateBillNumber(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        bill.AddDomainEvent(new BillGeneratedEvent(bill.Id, unitId, baseAmount, referenceMonth));

        return bill;
    }

    private static string GenerateBillNumber()
    {
        return $"FAT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";
    }

    public void ApplyEarlyPaymentDiscount(decimal discountPercentage, int daysBeforeDue)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var daysUntilDue = DueDate.DayNumber - today.DayNumber;

        if (daysUntilDue >= daysBeforeDue && Status == BillStatus.Pending)
        {
            DiscountAmount = BaseAmount * (discountPercentage / 100);
            DiscountType = "early_payment";
            TotalAmount = BaseAmount - DiscountAmount;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void CalculateFine(DateTime referenceDate)
    {
        if (Status != BillStatus.Pending && Status != BillStatus.Overdue)
            return;

        var today = DateOnly.FromDateTime(referenceDate);

        if (today <= DueDate)
            return;

        var daysOverdue = today.DayNumber - DueDate.DayNumber;

        // Multa única
        FineAmount = BaseAmount * (LateFeePercentage / 100);

        // Juros diários
        var maxInterestDays = MaxInterestMonths * 30;
        var interestDays = Math.Min(daysOverdue, maxInterestDays);
        InterestAmount = BaseAmount * (LateInterestDaily / 100) * interestDays;

        TotalAmount = BaseAmount - DiscountAmount + FineAmount + InterestAmount;
        Status = BillStatus.Overdue;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new FineCalculatedEvent(Id, UnitId, FineAmount, InterestAmount, daysOverdue));
    }

    public void Pay(decimal amount, string paymentMethod, string transactionId, Guid paidBy)
    {
        if (Status == BillStatus.Paid || Status == BillStatus.Cancelled || Status == BillStatus.Waived)
            throw new DomainException("Fatura já está paga, cancelada ou perdoada");

        PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow);
        PaymentAmount = amount;
        PaymentMethod = paymentMethod;
        TransactionId = transactionId;
        PaidBy = paidBy;

        if (amount >= TotalAmount)
        {
            Status = BillStatus.Paid;
        }
        else
        {
            Status = BillStatus.PartiallyPaid;
            Balance = TotalAmount - amount;
        }

        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new BillPaidEvent(Id, UnitId, amount, DateTime.UtcNow));
    }

    public void Cancel(string reason)
    {
        if (Status == BillStatus.Paid)
            throw new DomainException("Não é possível cancelar uma fatura já paga");

        Status = BillStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Waive()
    {
        if (Status == BillStatus.Paid)
            throw new DomainException("Não é possível perdoar uma fatura já paga");

        Status = BillStatus.Waived;
        TotalAmount = 0;
        FineAmount = 0;
        InterestAmount = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetBoleto(string url, string code)
    {
        BoletoUrl = url;
        BoletoCode = code;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPix(string code, string qrCodeUrl)
    {
        PixCode = code;
        PixQrCodeUrl = qrCodeUrl;
        UpdatedAt = DateTime.UtcNow;
    }
}