using CondoSync.Core.Enums;

namespace CondoSync.Core.Entities;

public class Service : AggregateRoot<Guid>, ITenantEntity
{
    public Guid CondominiumId { get; private set; }

    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string? Description { get; private set; }
    public string? Icon { get; private set; }
    public string Category { get; private set; }

    // Configuração
    public ServiceType ServiceType { get; private set; }

    // Disponibilidade
    public bool RequiresApproval { get; private set; }
    public bool RequiresPayment { get; private set; }
    public int? MaxBookingPerDay { get; private set; }
    public int? MaxBookingPerUser { get; private set; }
    public int AdvanceBookingDays { get; private set; }
    public int CancelBeforeHours { get; private set; }
    public bool AllowSimultaneous { get; private set; }

    // Horários
    public string? AvailableDays { get; private set; }
    public TimeOnly? AvailableTimeStart { get; private set; }
    public TimeOnly? AvailableTimeEnd { get; private set; }
    public int? SlotDuration { get; private set; }
    public bool AllowCustomTime { get; private set; }

    // Bloqueios
    public string? BlockedDates { get; private set; }

    // Preços
    public decimal Price { get; private set; }
    public string? PriceUnit { get; private set; }

    // Regras
    public string? Rules { get; private set; }
    public string? TermsOfUse { get; private set; }

    // Status
    public bool IsActive { get; private set; }
    public int DisplayOrder { get; private set; }

    // Imagens
    public string? Images { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private Service() { }

    public static Service Create(
        Guid condominiumId,
        string name,
        string slug,
        string category,
        ServiceType serviceType,
        string? description = null,
        decimal price = 0,
        bool requiresApproval = false,
        bool requiresPayment = false)
    {
        return new Service
        {
            Id = Guid.NewGuid(),
            CondominiumId = condominiumId,
            Name = name,
            Slug = slug.ToLowerInvariant().Trim(),
            Category = category,
            ServiceType = serviceType,
            Description = description,
            Price = price,
            RequiresApproval = requiresApproval,
            RequiresPayment = requiresPayment,
            CancelBeforeHours = 24,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? description = null, decimal? price = null,
        bool? requiresApproval = null)
    {
        Name = name;
        if (description != null) Description = description;
        if (price.HasValue) Price = price.Value;
        if (requiresApproval.HasValue) RequiresApproval = requiresApproval.Value;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void BlockDate(string date)
    {
        var blockedDates = string.IsNullOrEmpty(BlockedDates)
            ? new List<string>()
            : System.Text.Json.JsonSerializer.Deserialize<List<string>>(BlockedDates);

        blockedDates!.Add(date);
        BlockedDates = System.Text.Json.JsonSerializer.Serialize(blockedDates);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnblockDate(string date)
    {
        if (string.IsNullOrEmpty(BlockedDates)) return;

        var blockedDates = System.Text.Json.JsonSerializer.Deserialize<List<string>>(BlockedDates);
        blockedDates!.Remove(date);
        BlockedDates = System.Text.Json.JsonSerializer.Serialize(blockedDates);
        UpdatedAt = DateTime.UtcNow;
    }
}