using CondoSync.Core.Exceptions;

namespace CondoSync.Core.Entities;

public class GuestList : AggregateRoot<Guid>, ITenantEntity
{
    public Guid CondominiumId { get; private set; }
    public Guid CreatedBy { get; private set; }
    public Guid? BookingId { get; private set; }
    public Guid? UnitId { get; private set; }

    public string Title { get; private set; }
    public string? Description { get; private set; }
    public DateOnly EventDate { get; private set; }
    public TimeOnly? StartTime { get; private set; }
    public TimeOnly? EndTime { get; private set; }
    public int MaxGuests { get; private set; }
    public bool RequiresQrCode { get; private set; }

    public string Status { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private GuestList() { }

    public static GuestList Create(
        Guid condominiumId,
        Guid createdBy,
        string title,
        DateOnly eventDate,
        Guid? bookingId = null,
        Guid? unitId = null,
        string? description = null,
        TimeOnly? startTime = null,
        TimeOnly? endTime = null,
        int maxGuests = 50,
        bool requiresQrCode = true)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Título da lista de convidados não pode ser vazio");

        return new GuestList
        {
            Id = Guid.NewGuid(),
            CondominiumId = condominiumId,
            CreatedBy = createdBy,
            BookingId = bookingId,
            UnitId = unitId,
            Title = title,
            Description = description,
            EventDate = eventDate,
            StartTime = startTime,
            EndTime = endTime,
            MaxGuests = maxGuests,
            RequiresQrCode = requiresQrCode,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string title, string? description, TimeOnly? startTime, TimeOnly? endTime, int maxGuests, bool requiresQrCode)
    {
        Title = title;
        if (description != null) Description = description;
        StartTime = startTime;
        EndTime = endTime;
        MaxGuests = maxGuests;
        RequiresQrCode = requiresQrCode;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = "Cancelled";
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
