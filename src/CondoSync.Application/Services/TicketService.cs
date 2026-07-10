using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CondoSync.Application.Services;

public class TicketService
{
    private readonly IRepository<Ticket> _ticketRepository;
    private readonly IRepository<TicketMessage> _ticketMessageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<TicketService> _logger;

    public TicketService(
        IRepository<Ticket> ticketRepository,
        IRepository<TicketMessage> ticketMessageRepository,
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        ILogger<TicketService> logger)
    {
        _ticketRepository = ticketRepository;
        _ticketMessageRepository = ticketMessageRepository;
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantProvider.GetCurrentTenantId()
            ?? throw new UnauthorizedAccessException("Tenant não identificado");
    }

    public async Task<List<Ticket>> GetTicketsAsync(
        string? status = null, string? priority = null, string? category = null,
        Guid? assignedTo = null, Guid? unitId = null, int page = 1, int perPage = 20)
    {
        var tenantId = GetCurrentTenantId();
        var tickets = await _ticketRepository.FindAsync(t => t.CondominiumId == tenantId);

        var query = tickets.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            var ticketStatus = Enum.Parse<TicketStatus>(status, true);
            query = query.Where(t => t.Status == ticketStatus);
        }

        if (!string.IsNullOrWhiteSpace(priority))
        {
            var ticketPriority = Enum.Parse<TicketPriority>(priority, true);
            query = query.Where(t => t.Priority == ticketPriority);
        }

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(t => t.Category == category);

        if (assignedTo.HasValue)
            query = query.Where(t => t.AssignedTo == assignedTo.Value);

        if (unitId.HasValue)
            query = query.Where(t => t.UnitId == unitId.Value);

        return query
            .OrderByDescending(t => t.Priority)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToList();
    }

    public async Task<Ticket?> GetTicketByIdAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var tickets = await _ticketRepository.FindAsync(t => t.Id == id && t.CondominiumId == tenantId);
        return tickets.FirstOrDefault();
    }

    public async Task<Ticket> CreateTicketAsync(
        Guid unitId, Guid residentId, string title, string description,
        string category, string priority = "Normal", string? subcategory = null)
    {
        var tenantId = GetCurrentTenantId();
        var ticketPriority = Enum.Parse<TicketPriority>(priority, true);

        var ticket = Ticket.Create(
            tenantId, unitId, residentId,
            title, description, category,
            ticketPriority, subcategory);

        await _ticketRepository.AddAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Ticket criado: {TicketNumber} - {Title}", ticket.TicketNumber, title);

        return ticket;
    }

    public async Task<Ticket?> UpdateStatusAsync(Guid id, string newStatus, Guid? resolvedBy = null, string? resolution = null)
    {
        var ticket = await GetTicketByIdAsync(id);
        if (ticket == null) return null;

        var status = Enum.Parse<TicketStatus>(newStatus, true);
        ticket.UpdateStatus(status, resolvedBy, resolution);
        await _unitOfWork.SaveChangesAsync();

        return ticket;
    }

    public async Task<TicketMessage?> AddMessageAsync(Guid ticketId, Guid senderId, string message, bool isInternal = false)
    {
        var ticket = await GetTicketByIdAsync(ticketId);
        if (ticket == null) return null;

        var ticketMessage = TicketMessage.Create(ticketId, senderId, message, isInternal);
        await _ticketMessageRepository.AddAsync(ticketMessage);
        await _unitOfWork.SaveChangesAsync();

        return ticketMessage;
    }
}