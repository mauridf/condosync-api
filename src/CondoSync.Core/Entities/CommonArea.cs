using CondoSync.Core.Exceptions;

namespace CondoSync.Core.Entities;

public class CommonArea : AggregateRoot<Guid>, ITenantEntity
{
    public Guid CondominiumId { get; private set; }

    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string Type { get; private set; }

    // Capacidade
    public int? Capacity { get; private set; }
    public int MaxGuestsPerResident { get; private set; }

    // Regras
    public string? Rules { get; private set; }
    public bool RequiresBooking { get; private set; }
    public bool RequiresDeposit { get; private set; }
    public decimal? DepositAmount { get; private set; }

    // Horários
    public TimeOnly? OpenTime { get; private set; }
    public TimeOnly? CloseTime { get; private set; }
    public string? OperatingHours { get; private set; }

    // Status
    public bool IsActive { get; private set; }
    public string MaintenanceStatus { get; private set; }

    // Manutenção programada
    public string? ScheduledMaintenance { get; private set; }

    // Imagens
    public string? Images { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private CommonArea() { }

    public static CommonArea Create(
        Guid condominiumId,
        string name,
        string type,
        string? description = null,
        int? capacity = null,
        bool requiresBooking = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome da área comum não pode ser vazio");

        return new CommonArea
        {
            Id = Guid.NewGuid(),
            CondominiumId = condominiumId,
            Name = name,
            Type = type,
            Description = description,
            Capacity = capacity,
            MaxGuestsPerResident = 5,
            RequiresBooking = requiresBooking,
            IsActive = true,
            MaintenanceStatus = "operational",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? description = null, int? capacity = null,
        bool? requiresBooking = null)
    {
        Name = name;
        if (description != null) Description = description;
        if (capacity.HasValue) Capacity = capacity.Value;
        if (requiresBooking.HasValue) RequiresBooking = requiresBooking.Value;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetMaintenance(string description)
    {
        MaintenanceStatus = "under_maintenance";
        IsActive = false;

        var maintenance = string.IsNullOrEmpty(ScheduledMaintenance)
            ? new List<dynamic>()
            : System.Text.Json.JsonSerializer.Deserialize<List<dynamic>>(ScheduledMaintenance);

        maintenance!.Add(new { description, date = DateTime.UtcNow });
        ScheduledMaintenance = System.Text.Json.JsonSerializer.Serialize(maintenance);
        UpdatedAt = DateTime.UtcNow;
    }

    public void CompleteMaintenance()
    {
        MaintenanceStatus = "operational";
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetOperatingHours(string operatingHours)
    {
        OperatingHours = operatingHours;
        UpdatedAt = DateTime.UtcNow;
    }
}