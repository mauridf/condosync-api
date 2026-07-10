using System.Text.Json;
using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Exceptions;
using CondoSync.Core.Interfaces;
using CondoSync.Application.Features.Polls.DTOs;
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

    public async Task<List<Poll>> GetPollsAsync(string? status = null, string? category = null, int page = 1, int perPage = 20)
    {
        var tenantId = GetCurrentTenantId();
        var polls = await _pollRepository.FindAsync(p => p.CondominiumId == tenantId);

        var query = polls.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            var pollStatus = Enum.Parse<PollStatus>(status, true);
            query = query.Where(p => p.Status == pollStatus);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var isVotacao = category.Equals("votacao", StringComparison.OrdinalIgnoreCase);
            if (isVotacao)
                query = query.Where(p => p.VotingRule.HasValue);
            else
                query = query.Where(p => !p.VotingRule.HasValue);
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
        bool requiresUnitVote = false, string? description = null,
        string? votingRule = null, bool isBinding = false,
        string? voterSlug = null)
    {
        var tenantId = GetCurrentTenantId();
        var type = Enum.Parse<PollType>(pollType, true);
        var optionsJson = JsonSerializer.Serialize(options);

        VotingRule? parsedRule = null;
        if (!string.IsNullOrWhiteSpace(votingRule))
            parsedRule = Enum.Parse<VotingRule>(votingRule, true);

        var poll = Poll.Create(
            tenantId, createdBy, title, optionsJson,
            startsAt, endsAt, type, isAnonymous, requiresUnitVote,
            description, parsedRule, isBinding, voterSlug);

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

        if (poll.IsVotacao)
            await TallyVotesAsync(poll);

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
        var voteList = votes.ToList();

        var results = options.Select(o =>
        {
            var count = voteList.Count(v => v.SelectedOptions.Contains(o.Id));
            var percentage = poll.TotalVotes > 0
                ? Math.Round((decimal)count / poll.TotalVotes * 100, 1)
                : 0m;
            return new OptionResult(o.Id, o.Text, count, percentage, false);
        }).ToList();

        var maxVotes = results.MaxBy(r => r.VoteCount);
        if (maxVotes != null && maxVotes.VoteCount > 0)
        {
            results = results.Select(r => r with { IsWinning = r.OptionId == maxVotes.OptionId }).ToList();
        }

        return results;
    }

    public async Task<TallyResult?> GetTallyResultAsync(Guid id)
    {
        var poll = await GetPollByIdAsync(id);
        if (poll == null) return null;

        var votes = await _voteRepository.FindAsync(v => v.PollId == id);
        var options = JsonSerializer.Deserialize<List<PollOption>>(poll.Options) ?? new();
        var voteList = votes.ToList();

        var results = options.Select(o =>
        {
            var count = voteList.Count(v => v.SelectedOptions.Contains(o.Id));
            var percentage = poll.TotalVotes > 0
                ? Math.Round((decimal)count / poll.TotalVotes * 100, 1)
                : 0m;
            return new OptionResult(o.Id, o.Text, count, percentage, false);
        }).ToList();

        var maxVotes = results.MaxBy(r => r.VoteCount);
        if (maxVotes != null && maxVotes.VoteCount > 0)
        {
            results = results.Select(r => r with { IsWinning = r.OptionId == maxVotes.OptionId }).ToList();
        }

        return new TallyResult(
            poll.Id, poll.Title, poll.TotalVotes,
            poll.Status.ToString(), poll.IsVotacao ? "votacao" : "enquete",
            poll.VotingRule?.ToString(), poll.IsBinding,
            poll.ElectedCandidateId, poll.ApprovedOptionId,
            poll.VoterSlug, results);
    }

    public async Task<bool> DeletePollAsync(Guid id)
    {
        var poll = await GetPollByIdAsync(id);
        if (poll == null) return false;

        poll.SoftDelete();
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private async Task TallyVotesAsync(Poll poll)
    {
        var votes = await _voteRepository.FindAsync(v => v.PollId == poll.Id);
        var options = JsonSerializer.Deserialize<List<PollOption>>(poll.Options) ?? new();
        var voteList = votes.ToList();

        var tally = options.Select(o => new
        {
            Option = o,
            Count = voteList.Count(v => v.SelectedOptions.Contains(o.Id))
        }).ToList();

        var total = tally.Sum(t => t.Count);
        var winner = tally.OrderByDescending(t => t.Count).FirstOrDefault();

        if (winner == null || total == 0) return;

        var rule = poll.VotingRule ?? VotingRule.MajoritySimple;
        var majority = rule switch
        {
            VotingRule.MajoritySimple => total / 2m + 1,
            VotingRule.MajorityQualified => total * 0.6m,
            VotingRule.TwoThirds => total * 2m / 3m,
            VotingRule.AbsoluteMajority => total / 2m + 1,
            _ => total / 2m + 1
        };

        if (winner.Count >= majority)
        {
            if (poll.PollType == PollType.Single)
                poll.ElectCandidate(winner.Option.Id);
            else
                poll.ApproveOption(winner.Option.Id);
        }
    }

    public async Task<List<Poll>> GetEnquetesAsync(string? status = null, int page = 1, int perPage = 20)
    {
        return await GetPollsAsync(status, "enquete", page, perPage);
    }

    public async Task<List<Poll>> GetVotacoesAsync(string? status = null, int page = 1, int perPage = 20)
    {
        return await GetPollsAsync(status, "votacao", page, perPage);
    }
}
