using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using CondoSync.Core.Entities;
using CondoSync.Core.Events;
using CondoSync.Core.Interfaces;

namespace CondoSync.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly CondoSyncDbContext _context;
    private readonly IMediator _mediator;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;
    private bool _isSaving;

    public UnitOfWork(CondoSyncDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_isSaving)
            return await _context.SaveChangesAsync(cancellationToken);

        _isSaving = true;
        try
        {
            var events = ExtractDomainEvents();
            var result = await _context.SaveChangesAsync(cancellationToken);
            await PersistDomainEventsAsync(events, cancellationToken);
            await PublishDomainEventsAsync(events, cancellationToken);
            return result;
        }
        finally
        {
            _isSaving = false;
        }
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
            throw new InvalidOperationException("Já existe uma transação em andamento");

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
            throw new InvalidOperationException("Não há transação em andamento");

        if (_isSaving)
            return;

        _isSaving = true;
        try
        {
            var events = ExtractDomainEvents();
            await _context.SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
            await PersistDomainEventsAsync(events, cancellationToken);
            await PublishDomainEventsAsync(events, cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _isSaving = false;
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
            return;

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _currentTransaction?.Dispose();
            _context.Dispose();
        }
        _disposed = true;
    }

    // ─── Helpers ────────────────────────────────────────────────────

    private List<DomainEvent> ExtractDomainEvents()
    {
        var events = new List<DomainEvent>();

        foreach (var entry in _context.ChangeTracker.Entries<AggregateRoot<Guid>>())
        {
            if (entry.Entity.DomainEvents.Count > 0)
            {
                events.AddRange(entry.Entity.DomainEvents);
                entry.Entity.ClearDomainEvents();
            }
        }

        return events;
    }

    private async Task PersistDomainEventsAsync(List<DomainEvent> events, CancellationToken ct)
    {
        if (events.Count == 0) return;

        var entries = new List<EventStoreEntry>();

        foreach (var domainEvent in events)
        {
            var aggregateId = ResolveAggregateId(domainEvent);
            var aggregateType = ResolveAggregateType(domainEvent);

            var currentVersion = await GetCurrentVersionAsync(aggregateType, aggregateId, ct);

            var entry = EventStoreEntry.Create(
                aggregateType,
                aggregateId,
                currentVersion + 1,
                domainEvent.GetType().FullName ?? domainEvent.GetType().Name,
                JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                JsonSerializer.Serialize(new { occurred_at = domainEvent.OccurredAt }));

            entries.Add(entry);
        }

        _context.EventStore.AddRange(entries);
        await _context.SaveChangesAsync(ct);
    }

    private async Task PublishDomainEventsAsync(List<DomainEvent> events, CancellationToken ct)
    {
        foreach (var domainEvent in events)
        {
            await _mediator.Publish(domainEvent, ct);
        }
    }

    private static Guid ResolveAggregateId(DomainEvent domainEvent)
    {
        return domainEvent switch
        {
            BookingCreatedEvent e => e.BookingId,
            BookingApprovedEvent e => e.BookingId,
            BillGeneratedEvent e => e.BillId,
            BillPaidEvent e => e.BillId,
            FineCalculatedEvent e => e.BillId,
            TicketOpenedEvent e => e.TicketId,
            TicketResolvedEvent e => e.TicketId,
            NoticePublishedEvent e => e.NoticeId,
            _ => Guid.Empty
        };
    }

    private static string ResolveAggregateType(DomainEvent domainEvent)
    {
        return domainEvent switch
        {
            BookingCreatedEvent => "Booking",
            BookingApprovedEvent => "Booking",
            BillGeneratedEvent => "Bill",
            BillPaidEvent => "Bill",
            FineCalculatedEvent => "Bill",
            TicketOpenedEvent => "Ticket",
            TicketResolvedEvent => "Ticket",
            NoticePublishedEvent => "Notice",
            _ => "Unknown"
        };
    }

    private async Task<int> GetCurrentVersionAsync(string aggregateType, Guid aggregateId, CancellationToken ct)
    {
        var maxVersion = await _context.EventStore
            .Where(e => e.AggregateType == aggregateType && e.AggregateId == aggregateId)
            .MaxAsync(e => (int?)e.Version, ct);

        return maxVersion ?? 0;
    }
}
