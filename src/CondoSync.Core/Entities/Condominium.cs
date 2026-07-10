using CondoSync.Core.Enums;

namespace CondoSync.Core.Entities;

public class Condominium : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;
    public string? Cnpj { get; private set; }
    public string Slug { get; private set; } = default!;
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public string? ZipCode { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? LogoUrl { get; private set; }

    // Assinatura
    public SubscriptionPlan SubscriptionPlan { get; private set; }
    public SubscriptionStatus SubscriptionStatus { get; private set; }
    public DateTime? SubscriptionExpiresAt { get; private set; }
    public DateTime? TrialEndsAt { get; private set; }

    // Configurações
    public int MaxUnits { get; private set; }
    public int MaxResidentsPerUnit { get; private set; }
    public string Timezone { get; private set; } = default!;
    public string Language { get; private set; } = default!;

    // Módulos habilitados
    public List<string> EnabledModules { get; private set; } = default!;

    // Customização
    public string? Settings { get; private set; }
    public string? Features { get; private set; }

    // Auditoria
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private Condominium() { }

    public static Condominium Create(
        string name,
        string slug,
        string? cnpj = null,
        string? email = null,
        string? phone = null,
        SubscriptionPlan plan = SubscriptionPlan.Trial)
    {
        return new Condominium
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug.ToLowerInvariant().Trim(),
            Cnpj = cnpj,
            Email = email?.ToLowerInvariant(),
            Phone = phone,
            SubscriptionPlan = plan,
            SubscriptionStatus = plan == SubscriptionPlan.Trial ? SubscriptionStatus.Trial : SubscriptionStatus.Active,
            TrialEndsAt = plan == SubscriptionPlan.Trial ? DateTime.UtcNow.AddDays(15) : null,
            MaxUnits = plan == SubscriptionPlan.Trial ? 50 : 0, // 0 = ilimitado
            MaxResidentsPerUnit = 10,
            Timezone = "America/Sao_Paulo",
            Language = "pt-BR",
            EnabledModules = new List<string> { "units", "residents", "notices", "tickets" },
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? email = null, string? phone = null)
    {
        Name = name;
        if (email != null) Email = email.ToLowerInvariant();
        if (phone != null) Phone = phone;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAddress(string? address, string? city, string? state, string? zipCode)
    {
        Address = address;
        City = city;
        State = state;
        ZipCode = zipCode;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePlan(SubscriptionPlan newPlan)
    {
        SubscriptionPlan = newPlan;

        if (newPlan == SubscriptionPlan.Trial)
        {
            SubscriptionStatus = SubscriptionStatus.Trial;
            TrialEndsAt = DateTime.UtcNow.AddDays(15);
        }
        else
        {
            SubscriptionStatus = SubscriptionStatus.Active;
            TrialEndsAt = null;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void Suspend()
    {
        SubscriptionStatus = SubscriptionStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        SubscriptionStatus = SubscriptionStatus.Active;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        SubscriptionStatus = SubscriptionStatus.Cancelled;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLogo(string logoUrl)
    {
        LogoUrl = logoUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}