using System.Text.Json;
using CondoSync.Core.Entities;
using CondoSync.Core.Events;
using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CondoSync.Application.Services;

public class EventStoreService
{
    private readonly IRepository<EventStoreEntry> _eventStoreRepo;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<EventStoreService> _logger;

    public EventStoreService(
        IRepository<EventStoreEntry> eventStoreRepo,
        ITenantProvider tenantProvider,
        ILogger<EventStoreService> logger)
    {
        _eventStoreRepo = eventStoreRepo;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<List<EventStoreEntry>> GetEventsAsync(string? aggregateType = null, Guid? aggregateId = null,
        string? eventType = null, int page = 1, int perPage = 50)
    {
        var allEvents = await _eventStoreRepo.GetAllAsync();
        var query = allEvents.AsQueryable();

        if (!string.IsNullOrWhiteSpace(aggregateType))
            query = query.Where(e => e.AggregateType == aggregateType);
        if (aggregateId.HasValue)
            query = query.Where(e => e.AggregateId == aggregateId.Value);
        if (!string.IsNullOrWhiteSpace(eventType))
            query = query.Where(e => e.EventType == eventType);

        return query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToList();
    }

    public async Task<EventStoreEntry?> GetEventByIdAsync(long id)
    {
        var events = await _eventStoreRepo.FindAsync(e => e.Id == id);
        return events.FirstOrDefault();
    }

    public async Task<List<EventStoreEntry>> GetAggregateHistoryAsync(Guid aggregateId)
    {
        var events = await _eventStoreRepo.FindAsync(e => e.AggregateId == aggregateId);
        return events.OrderBy(e => e.Version).ToList();
    }

    public async Task<int> GetAggregateVersionAsync(Guid aggregateId)
    {
        var events = await _eventStoreRepo.FindAsync(e => e.AggregateId == aggregateId);
        return events.Any() ? events.Max(e => e.Version) : 0;
    }
}
