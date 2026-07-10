using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondoSync.Application.Features.Tickets.DTOs;
using CondoSync.Application.Services;
using CondoSync.Core.Entities;
using CondoSync.Core.Interfaces;

namespace CondoSync.Api.Controllers;

[Authorize(AuthenticationSchemes = "Tenant")]
public class TicketsController : BaseController
{
    private readonly TicketService _ticketService;
    private readonly IRepository<Resident> _residentRepo;

    public TicketsController(TicketService ticketService, IRepository<Resident> residentRepo)
    {
        _ticketService = ticketService;
        _residentRepo = residentRepo;
    }

    private async Task<Guid?> GetMyUnitId()
    {
        var userId = GetUserId();
        if (userId == null) return null;
        var residents = await _residentRepo.FindAsync(r =>
            r.UserId == userId && r.IsActive);
        return residents.Select(r => (Guid?)r.UnitId).FirstOrDefault();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null, [FromQuery] string? priority = null,
        [FromQuery] string? category = null, [FromQuery] Guid? assignedTo = null,
        [FromQuery] int page = 1, [FromQuery] int perPage = 20)
    {
        var tickets = await _ticketService.GetTicketsAsync(status, priority, category, assignedTo, page: page, perPage: perPage);
        return Ok(new { success = true, data = tickets, meta = new { page, perPage } });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var ticket = await _ticketService.GetTicketByIdAsync(id);
        return ticket == null ? NotFound() : Ok(new { success = true, data = ticket });
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyTickets()
    {
        var unitId = await GetMyUnitId();
        if (unitId == null)
            return NotFound(new { success = false, error = new { code = "RESIDENT_NOT_FOUND", message = "Perfil de morador não encontrado" } });

        var tickets = await _ticketService.GetTicketsAsync(unitId: unitId);
        return Ok(new { success = true, data = tickets });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTicketRequest request)
    {
        var ticket = await _ticketService.CreateTicketAsync(
            request.UnitId, request.ResidentId, request.Title, request.Description,
            request.Category, request.Priority, request.Subcategory);
        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, new { success = true, data = ticket });
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTicketStatusRequest request)
    {
        var ticket = await _ticketService.UpdateStatusAsync(id, request.Status, request.ResolvedBy, request.Resolution);
        return ticket == null ? NotFound() : Ok(new { success = true, message = "Status atualizado" });
    }

    [HttpPost("{id:guid}/messages")]
    public async Task<IActionResult> AddMessage(Guid id, [FromBody] AddTicketMessageRequest request)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return Unauthorized();

        var message = await _ticketService.AddMessageAsync(id, userId.Value, request.Message, request.IsInternal);
            return message == null ? NotFound() : Ok(new { success = true, data = message });
    }
}