using MediatR;

namespace CondoSync.Core.Events;

public abstract class DomainEvent : INotification
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}

// Eventos de Booking
public class BookingCreatedEvent : DomainEvent
{
    public Guid BookingId { get; }
    public Guid UnitId { get; }
    public Guid ResidentId { get; }
    public DateOnly Date { get; }

    public BookingCreatedEvent(Guid bookingId, Guid unitId, Guid residentId, DateOnly date)
    {
        BookingId = bookingId;
        UnitId = unitId;
        ResidentId = residentId;
        Date = date;
    }
}

public class BookingApprovedEvent : DomainEvent
{
    public Guid BookingId { get; }

    public BookingApprovedEvent(Guid bookingId)
    {
        BookingId = bookingId;
    }
}

// Eventos de Bill
public class BillGeneratedEvent : DomainEvent
{
    public Guid BillId { get; }
    public Guid UnitId { get; }
    public decimal Amount { get; }
    public string ReferenceMonth { get; }

    public BillGeneratedEvent(Guid billId, Guid unitId, decimal amount, string referenceMonth)
    {
        BillId = billId;
        UnitId = unitId;
        Amount = amount;
        ReferenceMonth = referenceMonth;
    }
}

public class BillPaidEvent : DomainEvent
{
    public Guid BillId { get; }
    public Guid UnitId { get; }
    public decimal AmountPaid { get; }
    public DateTime PaymentDate { get; }

    public BillPaidEvent(Guid billId, Guid unitId, decimal amountPaid, DateTime paymentDate)
    {
        BillId = billId;
        UnitId = unitId;
        AmountPaid = amountPaid;
        PaymentDate = paymentDate;
    }
}

// Eventos de Ticket
public class TicketOpenedEvent : DomainEvent
{
    public Guid TicketId { get; }
    public Guid UnitId { get; }
    public string Category { get; }
    public string Priority { get; }

    public TicketOpenedEvent(Guid ticketId, Guid unitId, string category, string priority)
    {
        TicketId = ticketId;
        UnitId = unitId;
        Category = category;
        Priority = priority;
    }
}

public class TicketResolvedEvent : DomainEvent
{
    public Guid TicketId { get; }
    public Guid ResolvedBy { get; }

    public TicketResolvedEvent(Guid ticketId, Guid resolvedBy)
    {
        TicketId = ticketId;
        ResolvedBy = resolvedBy;
    }
}

// Eventos de Notice
public class NoticePublishedEvent : DomainEvent
{
    public Guid NoticeId { get; }
    public string Category { get; }
    public bool IsUrgent { get; }

    public NoticePublishedEvent(Guid noticeId, string category, bool isUrgent)
    {
        NoticeId = noticeId;
        Category = category;
        IsUrgent = isUrgent;
    }
}

// Eventos Financeiros
public class FineCalculatedEvent : DomainEvent
{
    public Guid BillId { get; }
    public Guid UnitId { get; }
    public decimal FineAmount { get; }
    public decimal InterestAmount { get; }
    public int DaysOverdue { get; }

    public FineCalculatedEvent(Guid billId, Guid unitId, decimal fineAmount, decimal interestAmount, int daysOverdue)
    {
        BillId = billId;
        UnitId = unitId;
        FineAmount = fineAmount;
        InterestAmount = interestAmount;
        DaysOverdue = daysOverdue;
    }
}