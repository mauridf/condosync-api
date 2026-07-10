using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondoSync.Application.Features.Notices.DTOs;
using CondoSync.Application.Services;

namespace CondoSync.Api.Controllers;

[Authorize(AuthenticationSchemes = "Tenant")]
public class NoticesController : BaseController
{
    private readonly NoticeService _noticeService;

    public NoticesController(NoticeService noticeService) => _noticeService = noticeService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category = null, [FromQuery] bool? isPinned = null,
        [FromQuery] int page = 1, [FromQuery] int perPage = 20)
    {
        var notices = await _noticeService.GetNoticesAsync(category, isPinned, page, perPage);
        return Ok(new { success = true, data = notices, meta = new { page, perPage } });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var notice = await _noticeService.GetNoticeByIdAsync(id);
        return notice == null ? NotFound() : Ok(new { success = true, data = notice });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNoticeRequest request)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return Unauthorized();

        var notice = await _noticeService.CreateNoticeAsync(
            userId.Value, request.Title, request.Content,
            request.Category, request.IsUrgent, request.Summary, request.Visibility);

        return CreatedAtAction(nameof(GetById), new { id = notice.Id }, new { success = true, data = notice });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateNoticeRequest request)
    {
        var notice = await _noticeService.UpdateNoticeAsync(id, request.Title, request.Content,
            request.Summary, request.Category, request.IsUrgent);
        return notice == null ? NotFound() : Ok(new { success = true, data = notice });
    }

    [HttpPatch("{id:guid}/pin")]
    public async Task<IActionResult> TogglePin(Guid id)
    {
        var result = await _noticeService.TogglePinAsync(id);
        return result ? Ok(new { success = true, message = "Fixação alterada" }) : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _noticeService.DeleteNoticeAsync(id);
        return result ? Ok(new { success = true, message = "Aviso removido" }) : NotFound();
    }

    [HttpPost("{id:guid}/comments")]
    public async Task<IActionResult> AddComment(Guid id, [FromBody] AddCommentRequest request)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return Unauthorized();

        var comment = await _noticeService.AddCommentAsync(id, userId.Value, request.Content);
        return Ok(new { success = true, data = comment });
    }

    [HttpPost("{id:guid}/reactions")]
    public async Task<IActionResult> AddReaction(Guid id, [FromBody] AddReactionRequest request)
    {
        var result = await _noticeService.AddReactionAsync(id, request.ReactionType);
            return result ? Ok(new { success = true, message = "Reação adicionada" }) : NotFound();
    }
}