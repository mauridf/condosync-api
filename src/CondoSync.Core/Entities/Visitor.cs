using CondoSync.Core.Enums;
using CondoSync.Core.Exceptions;

namespace CondoSync.Core.Entities;

public class Visitor : AggregateRoot<Guid>, ITenantEntity
{
    public Guid CondominiumId { get; private set; }
    public Guid UnitId { get; private set; }
    public Guid? ResidentId { get; private set; }

    // Dados
    public string Name { get; private set; }
    public string? Document { get; private set; }
    public string? DocumentType { get; private set; }
    public string? VehiclePlate { get; private set; }
    public string? VehicleModel { get; private set; }
    public string? Phone { get; private set; }

    // Visita
    public DateOnly VisitDate { get; private set; }
    public DateTime? EntryTime { get; private set; }
    public DateTime? ExitTime { get; private set; }
    public TimeOnly? ExpectedEntryTime { get; private set; }
    public TimeOnly? ExpectedExitTime { get; private set; }

    // Tipo
    public VisitorType VisitorType { get; private set; }

    // Empresa (se prestador de serviço)
    public string? CompanyName { get; private set; }
    public string? ServiceDescription { get; private set; }

    // Autorização
    public string? AuthorizationCode { get; private set; }
    public string? QrCodeUrl { get; private set; }

    // Recorrência
    public bool IsRecurring { get; private set; }
    public string? RecurringSchedule { get; private set; }

    // Status
    public VisitorStatus Status { get; private set; }

    // Observações
    public string? Notes { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Visitor() { }

    public static Visitor Create(
        Guid condominiumId,
        Guid unitId,
        string name,
        DateOnly visitDate,
        VisitorType visitorType = VisitorType.Guest,
        Guid? residentId = null,
        string? document = null,
        string? phone = null,
        TimeOnly? expectedEntryTime = null,
        TimeOnly? expectedExitTime = null,
        string? companyName = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do visitante não pode ser vazio");

        if (visitDate < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new DomainException("Data da visita não pode ser no passado");

        var visitor = new Visitor
        {
            Id = Guid.NewGuid(),
            CondominiumId = condominiumId,
            UnitId = unitId,
            Name = name,
            VisitDate = visitDate,
            VisitorType = visitorType,
            ResidentId = residentId,
            Document = document,
            Phone = phone,
            ExpectedEntryTime = expectedEntryTime,
            ExpectedExitTime = expectedExitTime,
            CompanyName = companyName,
            Notes = notes,
            Status = VisitorStatus.Authorized,
            AuthorizationCode = GenerateAuthorizationCode(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return visitor;
    }

    private static string GenerateAuthorizationCode()
    {
        return Guid.NewGuid().ToString()[..8].ToUpper();
    }

    public void SetRecurring(string recurringSchedule)
    {
        IsRecurring = true;
        RecurringSchedule = recurringSchedule;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetQrCode(string qrCodeUrl)
    {
        QrCodeUrl = qrCodeUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Arrive()
    {
        if (Status != VisitorStatus.Authorized)
            throw new DomainException("Visitante não está autorizado");

        if (VisitDate != DateOnly.FromDateTime(DateTime.UtcNow))
            throw new DomainException("Visitante só pode entrar na data da visita");

        Status = VisitorStatus.Arrived;
        EntryTime = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Depart()
    {
        if (Status != VisitorStatus.Arrived)
            throw new DomainException("Visitante não está no local");

        Status = VisitorStatus.Departed;
        ExitTime = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == VisitorStatus.Departed || Status == VisitorStatus.Expired)
            throw new DomainException("Visita já finalizada ou expirada");

        Status = VisitorStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Expire()
    {
        if (Status == VisitorStatus.Arrived)
            throw new DomainException("Não é possível expirar visita em andamento");

        Status = VisitorStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string? phone = null, string? notes = null)
    {
        Name = name;
        if (phone != null) Phone = phone;
        if (notes != null) Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}