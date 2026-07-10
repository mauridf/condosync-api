namespace CondoSync.Core.Entities;

public class CondominiumSettings : AggregateRoot<Guid>, ITenantEntity
{
    public Guid CondominiumId { get; private set; }

    // Geral
    public bool AllowSelfRegistration { get; private set; }
    public bool RequireAdminApproval { get; private set; }
    public bool AllowGuestRegistration { get; private set; }
    public int MaxFamilyMembersPerUnit { get; private set; }
    public int MaxPetsPerUnit { get; private set; }

    // Financeiro
    public int InvoiceGenerationDay { get; private set; }
    public int DueDay { get; private set; }
    public decimal LateFeePercentage { get; private set; }
    public decimal LateInterestDaily { get; private set; }
    public decimal EarlyPaymentDiscountPercentage { get; private set; }
    public int EarlyPaymentDays { get; private set; }
    public bool AutomaticBoletoGeneration { get; private set; }
    public bool EnablePix { get; private set; }
    public bool EnableCreditCard { get; private set; }
    public string? PaymentGateway { get; private set; }

    // Notificações
    public string? NotificationEmailTemplate { get; private set; }
    public string? EmailFromName { get; private set; }
    public string? EmailFromAddress { get; private set; }
    public bool SmsEnabled { get; private set; }
    public string? SmsProvider { get; private set; }

    // Aparência
    public string PrimaryColor { get; private set; } = default!;
    public string SecondaryColor { get; private set; } = default!;
    public string? LogoUrl { get; private set; }
    public string? FaviconUrl { get; private set; }
    public string? CustomCss { get; private set; }

    // Visitantes
    public bool VisitorQrCodeEnabled { get; private set; }
    public bool VisitorNotifyOwner { get; private set; }
    public int MaxVisitorsPerDay { get; private set; }
    public bool VisitorAutoApprove { get; private set; }

    // Integrações
    public string? Integrations { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private CondominiumSettings() { }

    public static CondominiumSettings CreateDefault(Guid condominiumId)
    {
        return new CondominiumSettings
        {
            Id = Guid.NewGuid(),
            CondominiumId = condominiumId,
            AllowSelfRegistration = true,
            RequireAdminApproval = true,
            AllowGuestRegistration = true,
            MaxFamilyMembersPerUnit = 10,
            MaxPetsPerUnit = 3,
            InvoiceGenerationDay = 5,
            DueDay = 10,
            LateFeePercentage = 2.00m,
            LateInterestDaily = 0.033m,
            EarlyPaymentDiscountPercentage = 0,
            EarlyPaymentDays = 0,
            EnablePix = true,
            PrimaryColor = "#1976D2",
            SecondaryColor = "#FF9800",
            VisitorQrCodeEnabled = true,
            VisitorNotifyOwner = true,
            MaxVisitorsPerDay = 10,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateFinancialSettings(int invoiceGenerationDay, int dueDay,
        decimal lateFeePercentage, decimal lateInterestDaily)
    {
        InvoiceGenerationDay = invoiceGenerationDay;
        DueDay = dueDay;
        LateFeePercentage = lateFeePercentage;
        LateInterestDaily = lateInterestDaily;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAppearance(string primaryColor, string secondaryColor,
        string? logoUrl = null, string? faviconUrl = null)
    {
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        if (logoUrl != null) LogoUrl = logoUrl;
        if (faviconUrl != null) FaviconUrl = faviconUrl;
        UpdatedAt = DateTime.UtcNow;
    }
}