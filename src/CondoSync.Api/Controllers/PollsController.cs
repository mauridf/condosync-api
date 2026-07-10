using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondoSync.Application.Features.Polls.DTOs;
using CondoSync.Application.Services;

namespace CondoSync.Api.Controllers;

[Authorize(AuthenticationSchemes = "Tenant")]
public class PollsController : BaseController
{
    private readonly PollService _pollService;

    public PollsController(PollService pollService) => _pollService = pollService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null,
        [FromQuery] int page = 1, [FromQuery] int perPage = 20)
    {
        var polls = await _pollService.GetPollsAsync(status, page, perPage);
        return Ok(new { success = true, data = polls, meta = new { page, perPage } });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var poll = await _pollService.GetPollByIdAsync(id);
        return poll == null ? NotFound() : Ok(new { success = true, data = poll });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePollRequest request)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return Unauthorized();

        var poll = await _pollService.CreatePollAsync(
            userId.Value, request.Title, request.Options,
            request.StartsAt, request.EndsAt, request.PollType,
            request.IsAnonymous, request.RequiresUnitVote, request.Description);

        return CreatedAtAction(nameof(GetById), new { id = poll.Id }, new { success = true, data = poll });
    }

    [HttpPatch("{id:guid}/open")]
    public async Task<IActionResult> Open(Guid id)
    {
        var poll = await _pollService.OpenPollAsync(id);
        return poll == null ? NotFound() : Ok(new { success = true, message = "Enquete aberta" });
    }

    [HttpPatch("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id)
    {
        var poll = await _pollService.ClosePollAsync(id);
        return poll == null ? NotFound() : Ok(new { success = true, message = "Enquete encerrada" });
    }

    [HttpPost("{id:guid}/vote")]
    public async Task<IActionResult> Vote(Guid id, [FromBody] VoteRequest request)
    {
        try
        {
            var userId = GetUserId();
            var vote = await _pollService.VoteAsync(id, request.SelectedOptions, userId: userId);
            return Ok(new { success = true, data = vote });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = new { code = "VALIDATION_ERROR", message = ex.Message } });
        }
    }

    [HttpGet("{id:guid}/results")]
    public async Task<IActionResult> GetResults(Guid id)
    {
        var results = await _pollService.GetResultsAsync(id);
        return results == null ? NotFound() : Ok(new { success = true, data = results });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _pollService.DeletePollAsync(id);
        return result ? Ok(new { success = true, message = "Enquete removida" }) : NotFound();
    }
}