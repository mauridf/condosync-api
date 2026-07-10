using System.Text.Json;
using MediatR;
using CondoSync.Core.Entities;
using CondoSync.Core.Events;
using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CondoSync.Application.EventHandlers;

public class BookingCreatedEventHandler : INotificationHandler<BookingCreatedEvent>
{
    private readonly IRepository<ActivityLog> _activityRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BookingCreatedEventHandler> _logger;

    public BookingCreatedEventHandler(
        IRepository<ActivityLog> activityRepo,
        IUnitOfWork unitOfWork,
        ILogger<BookingCreatedEventHandler> logger)
    {
        _activityRepo = activityRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(BookingCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Booking created: {BookingId}", notification.BookingId);

        var log = ActivityLog.Create(
            Guid.Empty, "BookingCreated", "Booking",
            notification.BookingId, newValues: JsonSerializer.Serialize(notification),
            details: $"Reserva {notification.BookingId} criada para unidade {notification.UnitId} em {notification.Date}");

        await _activityRepo.AddAsync(log);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class BookingApprovedEventHandler : INotificationHandler<BookingApprovedEvent>
{
    private readonly IRepository<ActivityLog> _activityRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BookingApprovedEventHandler> _logger;

    public BookingApprovedEventHandler(
        IRepository<ActivityLog> activityRepo,
        IUnitOfWork unitOfWork,
        ILogger<BookingApprovedEventHandler> logger)
    {
        _activityRepo = activityRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(BookingApprovedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Booking approved: {BookingId}", notification.BookingId);

        var log = ActivityLog.Create(
            Guid.Empty, "BookingApproved", "Booking",
            notification.BookingId, newValues: JsonSerializer.Serialize(notification),
            details: $"Reserva {notification.BookingId} aprovada");

        await _activityRepo.AddAsync(log);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class BillGeneratedEventHandler : INotificationHandler<BillGeneratedEvent>
{
    private readonly IRepository<ActivityLog> _activityRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BillGeneratedEventHandler> _logger;

    public BillGeneratedEventHandler(
        IRepository<ActivityLog> activityRepo,
        IUnitOfWork unitOfWork,
        ILogger<BillGeneratedEventHandler> logger)
    {
        _activityRepo = activityRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(BillGeneratedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Bill generated: {BillId}, Amount: {Amount}", notification.BillId, notification.Amount);

        var log = ActivityLog.Create(
            Guid.Empty, "BillGenerated", "Bill",
            notification.BillId, newValues: JsonSerializer.Serialize(notification),
            details: $"Fatura {notification.BillId} gerada - R$ {notification.Amount:F2} - Ref: {notification.ReferenceMonth}");

        await _activityRepo.AddAsync(log);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class BillPaidEventHandler : INotificationHandler<BillPaidEvent>
{
    private readonly IRepository<ActivityLog> _activityRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BillPaidEventHandler> _logger;

    public BillPaidEventHandler(
        IRepository<ActivityLog> activityRepo,
        IUnitOfWork unitOfWork,
        ILogger<BillPaidEventHandler> logger)
    {
        _activityRepo = activityRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(BillPaidEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Bill paid: {BillId}, Amount: {Amount}", notification.BillId, notification.AmountPaid);

        var log = ActivityLog.Create(
            Guid.Empty, "BillPaid", "Bill",
            notification.BillId, newValues: JsonSerializer.Serialize(notification),
            details: $"Fatura {notification.BillId} paga - R$ {notification.AmountPaid:F2}");

        await _activityRepo.AddAsync(log);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class TicketOpenedEventHandler : INotificationHandler<TicketOpenedEvent>
{
    private readonly IRepository<ActivityLog> _activityRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TicketOpenedEventHandler> _logger;

    public TicketOpenedEventHandler(
        IRepository<ActivityLog> activityRepo,
        IUnitOfWork unitOfWork,
        ILogger<TicketOpenedEventHandler> logger)
    {
        _activityRepo = activityRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(TicketOpenedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Ticket opened: {TicketId}, Category: {Category}", notification.TicketId, notification.Category);

        var log = ActivityLog.Create(
            Guid.Empty, "TicketOpened", "Ticket",
            notification.TicketId, newValues: JsonSerializer.Serialize(notification),
            details: $"Chamado {notification.TicketId} aberto - Categoria: {notification.Category}");

        await _activityRepo.AddAsync(log);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class TicketResolvedEventHandler : INotificationHandler<TicketResolvedEvent>
{
    private readonly IRepository<ActivityLog> _activityRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TicketResolvedEventHandler> _logger;

    public TicketResolvedEventHandler(
        IRepository<ActivityLog> activityRepo,
        IUnitOfWork unitOfWork,
        ILogger<TicketResolvedEventHandler> logger)
    {
        _activityRepo = activityRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(TicketResolvedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Ticket resolved: {TicketId}", notification.TicketId);

        var log = ActivityLog.Create(
            Guid.Empty, "TicketResolved", "Ticket",
            notification.TicketId, newValues: JsonSerializer.Serialize(notification),
            details: $"Chamado {notification.TicketId} resolvido");

        await _activityRepo.AddAsync(log);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class NoticePublishedEventHandler : INotificationHandler<NoticePublishedEvent>
{
    private readonly IRepository<ActivityLog> _activityRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NoticePublishedEventHandler> _logger;

    public NoticePublishedEventHandler(
        IRepository<ActivityLog> activityRepo,
        IUnitOfWork unitOfWork,
        ILogger<NoticePublishedEventHandler> logger)
    {
        _activityRepo = activityRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(NoticePublishedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Notice published: {NoticeId}, Urgent: {IsUrgent}", notification.NoticeId, notification.IsUrgent);

        var log = ActivityLog.Create(
            Guid.Empty, "NoticePublished", "Notice",
            notification.NoticeId, newValues: JsonSerializer.Serialize(notification),
            details: $"Aviso {notification.NoticeId} publicado - Categoria: {notification.Category}");

        await _activityRepo.AddAsync(log);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class FineCalculatedEventHandler : INotificationHandler<FineCalculatedEvent>
{
    private readonly IRepository<ActivityLog> _activityRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FineCalculatedEventHandler> _logger;

    public FineCalculatedEventHandler(
        IRepository<ActivityLog> activityRepo,
        IUnitOfWork unitOfWork,
        ILogger<FineCalculatedEventHandler> logger)
    {
        _activityRepo = activityRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(FineCalculatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fine calculated for bill {BillId}: Fine={Fine}, Interest={Interest}",
            notification.BillId, notification.FineAmount, notification.InterestAmount);

        var log = ActivityLog.Create(
            Guid.Empty, "FineCalculated", "Bill",
            notification.BillId, newValues: JsonSerializer.Serialize(notification),
            details: $"Multa calculada para fatura {notification.BillId} - Multa: R$ {notification.FineAmount:F2}, Juros: R$ {notification.InterestAmount:F2}");

        await _activityRepo.AddAsync(log);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
