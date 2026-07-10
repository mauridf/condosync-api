using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondoSync.Application.Features.Invitations.DTOs;
using CondoSync.Application.Services;

namespace CondoSync.Api.Controllers;

[Authorize(AuthenticationSchemes = "Tenant")]
public class UnitInvitationsController : BaseController
{
    private readonly UnitInvitationService _service;

    public UnitInvitationsController(UnitInvitationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? unitId = null, [FromQuery] string? status = null)
    {
        var invitations = await _service.GetAllAsync(unitId, status);
        return Ok(new { success = true, data = invitations });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var invitation = await _service.GetByIdAsync(id);
        return invitation == null ? NotFound() : Ok(new { success = true, data = invitation });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInvitationRequest request)
    {
        var userId = GetUserId() ?? throw new UnauthorizedAccessException("Usuário não autenticado");
        var invitation = await _service.CreateAsync(userId, request);
        return CreatedAtAction(nameof(GetById), new { id = invitation.Id }, new { success = true, data = invitation });
    }

    [HttpPost("use")]
    [AllowAnonymous]
    public async Task<IActionResult> Use([FromBody] UseInvitationRequest request)
    {
        var invitation = await _service.UseAsync(request.InvitationCode);
        return invitation == null ? NotFound() : Ok(new { success = true, data = invitation });
    }

    [HttpPost("{id:guid}/revoke")]
    public async Task<IActionResult> Revoke(Guid id)
    {
        var result = await _service.RevokeAsync(id);
        return result ? Ok(new { success = true }) : NotFound();
    }
}
