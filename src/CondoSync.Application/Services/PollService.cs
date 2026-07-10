using System.Text.Json;
using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CondoSync.Application.Services;

public class PollService
{
    private readonly IRepository<Poll> _pollRepository;
    private readonly IRepository<PollVote> _voteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<PollService> _logger;

    public PollService(
        IRepository<Poll> pollRepository,
        IRepository<PollVote> voteRepository,
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        ILogger<PollService> logger)
    {
        _pollRepository = pollRepository;
        _voteRepository = voteRepository;
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantProvider.GetCurrentTenantId()
            ?? throw new UnauthorizedAccessException("Tenant não identificado");
    }

    public async Task<List<Poll>> GetPollsAsync(string? status = null, int page = 1, int perPage = 20)
    {
        var tenantId = GetCurrentTenantId();
        var polls = await _pollRepository.FindAsync(p => p.CondominiumId == tenantId);

        var query = polls.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            var pollStatus = Enum.Parse<PollStatus>(status, true);
            query = query.Where(p => p.Status == pollStatus);
        }

        return query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToList();
    }

    public async Task<Poll?> GetPollByIdAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var polls = await _pollRepository.FindAsync(p => p.Id == id && p.CondominiumId == tenantId);
        return polls.FirstOrDefault();
    }

    public async Task<Poll> CreatePollAsync(
        Guid createdBy, string title, List<PollOption> options,
        DateTime startsAt, DateTime endsAt,
        string pollType = "Single", bool isAnonymous = false,
        bool requiresUnitVote = false, string? description = null)
    {
        var tenantId = GetCurrentTenantId();
        var type = Enum.Parse<PollType>(pollType, true);
        var optionsJson = JsonSerializer.Serialize(options);

        var poll = Poll.Create(
            tenantId, createdBy, title, optionsJson,
            startsAt, endsAt, type, isAnonymous, requiresUnitVote, description);

        await _pollRepository.AddAsync(poll);
        await _unitOfWork.SaveChangesAsync();

        return poll;
    }

    public async Task<Poll?> OpenPollAsync(Guid id)
    {
        var poll = await GetPollByIdAsync(id);
        if (poll == null) return null;

        poll.Open();
        await _unitOfWork.SaveChangesAsync();

        return poll;
    }

    public async Task<Poll?> ClosePollAsync(Guid id)
    {
        var poll = await GetPollByIdAsync(id);
        if (poll == null) return null;

        poll.Close();
        await _unitOfWork.SaveChangesAsync();

        return poll;
    }

    public async Task<PollVote> VoteAsync(
        Guid pollId, List<Guid> selectedOptions,
        Guid? unitId = null, Guid? residentId = null, Guid? userId = null)
    {
        var poll = await GetPollByIdAsync(pollId);
        if (poll == null)
            throw new InvalidOperationException("Enquete não encontrada");

        if (poll.Status != PollStatus.Active)
            throw new InvalidOperationException("Enquete não está ativa");

        if (DateTime.UtcNow < poll.StartsAt)
            throw new InvalidOperationException("Enquete ainda não começou");

        if (DateTime.UtcNow > poll.EndsAt)
            throw new InvalidOperationException("Enquete já terminou");

        var vote = PollVote.Create(pollId, selectedOptions, unitId, residentId, userId);
        await _voteRepository.AddAsync(vote);

        poll.RecordVote();
        await _unitOfWork.SaveChangesAsync();

        return vote;
    }

    public async Task<object?> GetResultsAsync(Guid id)
    {
        var poll = await GetPollByIdAsync(id);
        if (poll == null) return null;

        var votes = await _voteRepository.FindAsync(v => v.PollId == id);
        var options = JsonSerializer.Deserialize<List<PollOption>>(poll.Options) ?? new();

        var results = options.Select(o => new
        {
            OptionId = o.Id,
            OptionText = o.Text,
            VoteCount = votes.Count(v => v.SelectedOptions.Contains(o.Id)),
            Percentage = poll.TotalVotes > 0
                ? Math.Round((double)votes.Count(v => v.SelectedOptions.Contains(o.Id)) / poll.TotalVotes * 100, 1)
                : 0
        });

        return new
        {
            PollId = poll.Id,
            poll.Title,
            poll.TotalVotes,
            poll.Status,
            Results = results
        };
    }

    public async Task<bool> DeletePollAsync(Guid id)
    {
        var poll = await GetPollByIdAsync(id);
        if (poll == null) return false;

        poll.SoftDelete();
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}

public record PollOption(Guid Id, string Text, int Order);