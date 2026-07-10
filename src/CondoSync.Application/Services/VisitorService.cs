using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CondoSync.Application.Services;

public class VisitorService
{
    private readonly IRepository<Visitor> _visitorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<VisitorService> _logger;

    public VisitorService(
        IRepository<Visitor> visitorRepository,
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        ILogger<VisitorService> logger)
    {
        _visitorRepository = visitorRepository;
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantProvider.GetCurrentTenantId()
            ?? throw new UnauthorizedAccessException("Tenant não identificado");
    }

    public async Task<List<Visitor>> GetVisitorsAsync(Guid? unitId = null, DateTime? date = null)
    {
        var tenantId = GetCurrentTenantId();
        var visitors = await _visitorRepository.FindAsync(v => v.CondominiumId == tenantId);

        var query = visitors.AsQueryable();

        if (unitId.HasValue)
            query = query.Where(v => v.UnitId == unitId.Value);

        if (date.HasValue)
        {
            var dateOnly = DateOnly.FromDateTime(date.Value);
            query = query.Where(v => v.VisitDate == dateOnly);
        }

        return query.OrderByDescending(v => v.VisitDate).ToList();
    }

    public async Task<Visitor> AuthorizeVisitorAsync(
        Guid unitId, string name, DateOnly visitDate,
        VisitorType visitorType = VisitorType.Guest,
        Guid? residentId = null, string? phone = null,
        string? companyName = null, string? notes = null)
    {
        var tenantId = GetCurrentTenantId();

        var visitor = Visitor.Create(
            tenantId, unitId, name, visitDate,
            visitorType, residentId, phone: phone,
            companyName: companyName, notes: notes);

        await _visitorRepository.AddAsync(visitor);
        await _unitOfWork.SaveChangesAsync();

        return visitor;
    }

    public async Task<Visitor?> RegisterEntryAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var visitors = await _visitorRepository.FindAsync(v => v.Id == id && v.CondominiumId == tenantId);
        var visitor = visitors.FirstOrDefault();

        if (visitor == null) return null;

        visitor.Arrive();
        await _unitOfWork.SaveChangesAsync();

        return visitor;
    }

    public async Task<Visitor?> RegisterExitAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var visitors = await _visitorRepository.FindAsync(v => v.Id == id && v.CondominiumId == tenantId);
        var visitor = visitors.FirstOrDefault();

        if (visitor == null) return null;

        visitor.Depart();
        await _unitOfWork.SaveChangesAsync();

        return visitor;
    }

    public async Task<Visitor?> CancelAuthorizationAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var visitors = await _visitorRepository.FindAsync(v => v.Id == id && v.CondominiumId == tenantId);
        var visitor = visitors.FirstOrDefault();

        if (visitor == null) return null;

        visitor.Cancel();
        await _unitOfWork.SaveChangesAsync();

        return visitor;
    }
}