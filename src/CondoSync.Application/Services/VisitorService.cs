using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Exceptions;
using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CondoSync.Application.Services;

public class VisitorService
{
    private readonly IRepository<Visitor> _visitorRepository;
    private readonly IRepository<GuestList> _guestListRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<VisitorService> _logger;

    public VisitorService(
        IRepository<Visitor> visitorRepository,
        IRepository<GuestList> guestListRepository,
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        ILogger<VisitorService> logger)
    {
        _visitorRepository = visitorRepository;
        _guestListRepository = guestListRepository;
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantProvider.GetCurrentTenantId()
            ?? throw new UnauthorizedAccessException("Tenant não identificado");
    }

    public async Task<List<Visitor>> GetVisitorsAsync(Guid? unitId = null, DateTime? date = null,
        string? status = null, string? search = null, int page = 1, int perPage = 20)
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

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(v => v.Status == Enum.Parse<VisitorStatus>(status, true));

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(v => v.Name.Contains(search) || (v.Phone != null && v.Phone.Contains(search)));

        return query
            .OrderByDescending(v => v.VisitDate)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToList();
    }

    public async Task<Visitor?> GetVisitorByIdAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var visitors = await _visitorRepository.FindAsync(v => v.Id == id && v.CondominiumId == tenantId);
        return visitors.FirstOrDefault();
    }

    public async Task<Visitor> AuthorizeVisitorAsync(
        Guid unitId, string name, DateOnly visitDate,
        VisitorType visitorType = VisitorType.Guest,
        Guid? residentId = null, string? phone = null,
        string? companyName = null, string? notes = null,
        string? document = null, string? documentType = null,
        string? vehiclePlate = null, string? vehicleModel = null,
        TimeOnly? expectedEntryTime = null, TimeOnly? expectedExitTime = null,
        Guid? guestListId = null)
    {
        var tenantId = GetCurrentTenantId();

        var visitor = Visitor.Create(
            tenantId, unitId, name, visitDate,
            visitorType, residentId, document, phone,
            expectedEntryTime, expectedExitTime,
            companyName, notes, guestListId);

        if (!string.IsNullOrWhiteSpace(vehiclePlate))
        {
            // Veículo info setado via reflection no construtor não disponível, setar manualmente
            typeof(Visitor).GetProperty(nameof(Visitor.VehiclePlate))?.SetValue(visitor, vehiclePlate);
            typeof(Visitor).GetProperty(nameof(Visitor.VehicleModel))?.SetValue(visitor, vehicleModel);
        }

        await _visitorRepository.AddAsync(visitor);
        await _unitOfWork.SaveChangesAsync();

        return visitor;
    }

    public async Task<Visitor?> UpdateVisitorAsync(Guid id, string name, string? phone = null, string? notes = null)
    {
        var visitor = await GetVisitorByIdAsync(id);
        if (visitor == null) return null;

        visitor.Update(name, phone, notes);
        await _unitOfWork.SaveChangesAsync();
        return visitor;
    }

    public async Task<Visitor?> RegisterEntryAsync(Guid id)
    {
        var visitor = await GetVisitorByIdAsync(id);
        if (visitor == null) return null;

        visitor.Arrive();
        await _unitOfWork.SaveChangesAsync();
        return visitor;
    }

    public async Task<Visitor?> RegisterExitAsync(Guid id)
    {
        var visitor = await GetVisitorByIdAsync(id);
        if (visitor == null) return null;

        visitor.Depart();
        await _unitOfWork.SaveChangesAsync();
        return visitor;
    }

    public async Task<Visitor?> CancelAuthorizationAsync(Guid id)
    {
        var visitor = await GetVisitorByIdAsync(id);
        if (visitor == null) return null;

        visitor.Cancel();
        await _unitOfWork.SaveChangesAsync();
        return visitor;
    }

    public async Task<bool> DeleteVisitorAsync(Guid id)
    {
        var visitor = await GetVisitorByIdAsync(id);
        if (visitor == null) return false;

        visitor.Cancel();
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<Visitor?> LinkToGuestListAsync(Guid visitorId, Guid guestListId)
    {
        var visitor = await GetVisitorByIdAsync(visitorId);
        if (visitor == null) return null;

        var tenantId = GetCurrentTenantId();
        var guestLists = await _guestListRepository.FindAsync(g => g.Id == guestListId && g.CondominiumId == tenantId);
        var guestList = guestLists.FirstOrDefault();
        if (guestList == null) return null;

        visitor.LinkToGuestList(guestListId);
        await _unitOfWork.SaveChangesAsync();
        return visitor;
    }

    // ─── GuestList ──────────────────────────────────────────────────

    public async Task<List<GuestList>> GetGuestListsAsync(Guid? unitId = null, DateOnly? eventDate = null)
    {
        var tenantId = GetCurrentTenantId();
        var lists = await _guestListRepository.FindAsync(g => g.CondominiumId == tenantId);
        var query = lists.AsQueryable();

        if (unitId.HasValue)
            query = query.Where(g => g.UnitId == unitId.Value);
        if (eventDate.HasValue)
            query = query.Where(g => g.EventDate == eventDate.Value);

        return query.OrderByDescending(g => g.EventDate).ToList();
    }

    public async Task<GuestList?> GetGuestListByIdAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var lists = await _guestListRepository.FindAsync(g => g.Id == id && g.CondominiumId == tenantId);
        return lists.FirstOrDefault();
    }

    public async Task<GuestList> CreateGuestListAsync(
        Guid createdBy, string title, DateOnly eventDate,
        Guid? bookingId = null, Guid? unitId = null,
        string? description = null, TimeOnly? startTime = null,
        TimeOnly? endTime = null, int maxGuests = 50,
        bool requiresQrCode = true)
    {
        var tenantId = GetCurrentTenantId();

        var guestList = GuestList.Create(
            tenantId, createdBy, title, eventDate,
            bookingId, unitId, description,
            startTime, endTime, maxGuests, requiresQrCode);

        await _guestListRepository.AddAsync(guestList);
        await _unitOfWork.SaveChangesAsync();
        return guestList;
    }

    public async Task<GuestList?> UpdateGuestListAsync(Guid id, string title, string? description,
        TimeOnly? startTime, TimeOnly? endTime, int maxGuests, bool requiresQrCode)
    {
        var guestList = await GetGuestListByIdAsync(id);
        if (guestList == null) return null;

        guestList.Update(title, description, startTime, endTime, maxGuests, requiresQrCode);
        await _unitOfWork.SaveChangesAsync();
        return guestList;
    }

    public async Task<bool> CancelGuestListAsync(Guid id)
    {
        var guestList = await GetGuestListByIdAsync(id);
        if (guestList == null) return false;

        guestList.Cancel();
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteGuestListAsync(Guid id)
    {
        var guestList = await GetGuestListByIdAsync(id);
        if (guestList == null) return false;

        guestList.SoftDelete();
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
