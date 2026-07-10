using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondoSync.Application.Features.Tickets.DTOs;
using CondoSync.Application.Services;

namespace CondoSync.Api.Controllers;

[Authorize(AuthenticationSchemes = "Tenant")]
public class TicketsController : BaseController
{
    private readonly TicketService _ticketService;

    public TicketsController(TicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null, [FromQuery] string? priority = null,
        [FromQuery] string? category = null, [FromQuery] Guid? assignedTo = null,
        [FromQuery] int page = 1, [FromQuery] int perPage = 20)
    {
        var tickets = await _ticketService.GetTicketsAsync(status, priority, category, assignedTo, page, perPage);
        return Ok(new { success = true, data = tickets, meta = new { page, perPage } });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var ticket = await _ticketService.GetTicketByIdAsync(id);
        return ticket == null ? NotFound() : Ok(new { success = true, data = ticket });
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