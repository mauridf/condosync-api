using CondoSync.Core.Enums;

namespace CondoSync.Core.Entities;

public class Resident : AggregateRoot<Guid>, ITenantEntity
{
    public Guid CondominiumId { get; private set; }
    public Guid UnitId { get; private set; }
    public Guid? UserId { get; private set; }

    // Tipo
    public ResidentType ResidentType { get; private set; }

    // Dados pessoais
    public string Name { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Cpf { get; private set; }
    public string? Rg { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public string? Profession { get; private set; }

    // Proprietário (se locatário)
    public string? OwnerName { get; private set; }
    public string? OwnerPhone { get; private set; }
    public string? OwnerEmail { get; private set; }

    // Período
    public DateOnly? MoveInDate { get; private set; }
    public DateOnly? MoveOutDate { get; private set; }

    // Status
    public bool IsActive { get; private set; }
    public bool IsPrimary { get; private set; }
    public bool IsEmergencyContact { get; private set; }

    // Veículos e Pets
    public string? Vehicles { get; private set; }
    public string? Pets { get; private set; }

    // Acesso
    public bool HasSystemAccess { get; private set; }
    public string? AccessCode { get; private set; }
    public DateTime? AccessGrantedAt { get; private set; }

    // Biometria
    public string? BiometricHash { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private Resident() { }

    public static Resident Create(
        Guid condominiumId,
        Guid unitId,
        string name,
        ResidentType residentType,
        string? email = null,
        string? phone = null,
        string? cpf = null,
        bool isPrimary = false)
    {
        return new Resident
        {
            Id = Guid.NewGuid(),
            CondominiumId = condominiumId,
            UnitId = unitId,
            Name = name,
            ResidentType = residentType,
            Email = email?.ToLowerInvariant(),
            Phone = phone,
            Cpf = cpf,
            IsPrimary = isPrimary,
            IsActive = true,
            MoveInDate = DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? email = null, string? phone = null,
        string? profession = null)
    {
        Name = name;
        if (email != null) Email = email.ToLowerInvariant();
        if (phone != null) Phone = phone;
        if (profession != null) Profession = profession;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAsPrimary()
    {
        IsPrimary = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnsetPrimary()
    {
        IsPrimary = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetOwnerInfo(string ownerName, string? ownerPhone = null, string? ownerEmail = null)
    {
        OwnerName = ownerName;
        OwnerPhone = ownerPhone;
        OwnerEmail = ownerEmail?.ToLowerInvariant();
        UpdatedAt = DateTime.UtcNow;
    }

    public void MoveOut(DateOnly moveOutDate)
    {
        MoveOutDate = moveOutDate;
        IsActive = false;
        HasSystemAccess = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void GrantSystemAccess(string accessCode)
    {
        HasSystemAccess = true;
        AccessCode = accessCode;
        AccessGrantedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RevokeSystemAccess()
    {
        HasSystemAccess = false;
        AccessCode = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddVehicle(string plate, string model, string? color = null, string? brand = null)
    {
        var vehicles = string.IsNullOrEmpty(Vehicles)
            ? new List<dynamic>()
            : System.Text.Json.JsonSerializer.Deserialize<List<dynamic>>(Vehicles);

        vehicles!.Add(new { plate, model, color, brand });
        Vehicles = System.Text.Json.JsonSerializer.Serialize(vehicles);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddPet(string name, string species, string? breed = null, string? color = null)
    {
        var pets = string.IsNullOrEmpty(Pets)
            ? new List<dynamic>()
            : System.Text.Json.JsonSerializer.Deserialize<List<dynamic>>(Pets);

        pets!.Add(new { name, species, breed, color });
        Pets = System.Text.Json.JsonSerializer.Serialize(pets);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAsEmergencyContact()
    {
        IsEmergencyContact = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void LinkUser(Guid userId)
    {
        UserId = userId;
        HasSystemAccess = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}