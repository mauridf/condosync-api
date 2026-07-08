using CondoSync.Core.Enums;
using CondoSync.Core.Events;
using CondoSync.Core.Exceptions;

namespace CondoSync.Core.Entities;

public class Booking : AggregateRoot<Guid>, ITenantEntity
{
    public Guid CondominiumId { get; private set; }
    public Guid ServiceId { get; private set; }
    public Guid UnitId { get; private set; }
    public Guid ResidentId { get; private set; }

    // Datas/Horários
    public DateOnly BookingDate { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }

    // Status
    public BookingStatus Status { get; private set; }

    // Detalhes
    public string? Title { get; private set; }
    public string? Description { get; private set; }
    public int GuestsCount { get; private set; }
    public string? SpecialRequirements { get; private set; }

    // Aprovação
    public Guid? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime? RejectedAt { get; private set; }

    // Cancelamento
    public Guid? CancelledBy { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public bool CancelledBySystem { get; private set; }

    // Pagamento
    public decimal? Amount { get; private set; }
    public PaymentStatus? PaymentStatus { get; private set; }
    public string? PaymentMethod { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public string? TransactionId { get; private set; }

    // Check-in/Check-out
    public DateTime? CheckedInAt { get; private set; }
    public DateTime? CheckedOutAt { get; private set; }
    public string? QrCodeUrl { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Booking() { }

    public static Booking Create(
        Guid condominiumId,
        Guid serviceId,
        Guid unitId,
        Guid residentId,
        DateOnly bookingDate,
        TimeOnly startTime,
        TimeOnly endTime,
        bool requiresApproval,
        decimal? amount = null,
        string? title = null,
        string? description = null,
        int guestsCount = 0)
    {
        // Validações de domínio
        if (bookingDate < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new DomainException("Não é possível reservar em datas passadas");

        if (startTime >= endTime)
            throw new DomainException("Horário de início deve ser anterior ao término");

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            CondominiumId = condominiumId,
            ServiceId = serviceId,
            UnitId = unitId,
            ResidentId = residentId,
            BookingDate = bookingDate,
            StartTime = startTime,
            EndTime = endTime,
            Status = requiresApproval ? BookingStatus.Pending : BookingStatus.Approved,
            Title = title,
            Description = description,
            GuestsCount = guestsCount,
            Amount = amount,
            PaymentStatus = amount > 0 ? PaymentStatus.Pending : PaymentStatus.NotRequired,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        booking.AddDomainEvent(new BookingCreatedEvent(booking.Id, unitId, residentId, bookingDate));

        return booking;
    }

    public void Approve(Guid approvedBy)
    {
        if (Status != BookingStatus.Pending)
            throw new DomainException("Apenas reservas pendentes podem ser aprovadas");

        Status = BookingStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new BookingApprovedEvent(Id));
    }

    public void Reject(Guid rejectedBy, string reason)
    {
        if (Status != BookingStatus.Pending)
            throw new DomainException("Apenas reservas pendentes podem ser rejeitadas");

        Status = BookingStatus.Rejected;
        RejectionReason = reason;
        RejectedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(Guid cancelledBy, string reason, bool bySystem = false)
    {
        if (Status is BookingStatus.Cancelled or BookingStatus.Completed or BookingStatus.NoShow)
            throw new DomainException("Reserva já cancelada, finalizada ou com no-show");

        Status = BookingStatus.Cancelled;
        CancelledBy = cancelledBy;
        CancellationReason = reason;
        CancelledAt = DateTime.UtcNow;
        CancelledBySystem = bySystem;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CheckIn()
    {
        if (Status != BookingStatus.Approved)
            throw new DomainException("Apenas reservas aprovadas podem fazer check-in");

        CheckedInAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CheckOut()
    {
        if (!CheckedInAt.HasValue)
            throw new DomainException("Check-in não realizado");

        Status = BookingStatus.Completed;
        CheckedOutAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsNoShow()
    {
        if (Status != BookingStatus.Approved)
            throw new DomainException("Apenas reservas aprovadas podem ser marcadas como no-show");

        Status = BookingStatus.NoShow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Pay(string paymentMethod, string transactionId)
    {
        if (Amount <= 0)
            throw new DomainException("Reserva não requer pagamento");

        PaymentStatus = PaymentStatus.Paid;
        PaymentMethod = paymentMethod;
        TransactionId = transactionId;
        PaidAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}