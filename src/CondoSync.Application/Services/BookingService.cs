using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CondoSync.Application.Services;

public class BookingService
{
    private readonly IRepository<Booking> _bookingRepo;
    private readonly IRepository<Service> _serviceRepo;
    private readonly IRepository<Resident> _residentRepo;
    private readonly ITenantProvider _tenantProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BookingService> _logger;

    public BookingService(
        IRepository<Booking> bookingRepo,
        IRepository<Service> serviceRepo,
        IRepository<Resident> residentRepo,
        ITenantProvider tenantProvider,
        IUnitOfWork unitOfWork,
        ILogger<BookingService> logger)
    {
        _bookingRepo = bookingRepo;
        _serviceRepo = serviceRepo;
        _residentRepo = residentRepo;
        _tenantProvider = tenantProvider;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantProvider.GetCurrentTenantId()
            ?? throw new UnauthorizedAccessException("Tenant não identificado");
    }

    public async Task<List<Booking>> GetBookingsAsync(
        Guid? serviceId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? status = null,
        int page = 1,
        int perPage = 20)
    {
        var tenantId = GetCurrentTenantId();
        var allBookings = (await _bookingRepo.GetAllAsync())
            .Where(b => b.CondominiumId == tenantId);

        if (serviceId.HasValue)
            allBookings = allBookings.Where(b => b.ServiceId == serviceId.Value);

        if (startDate.HasValue)
        {
            var dateOnly = DateOnly.FromDateTime(startDate.Value);
            allBookings = allBookings.Where(b => b.BookingDate >= dateOnly);
        }

        if (endDate.HasValue)
        {
            var dateOnly = DateOnly.FromDateTime(endDate.Value);
            allBookings = allBookings.Where(b => b.BookingDate <= dateOnly);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var bookingStatus = Enum.Parse<BookingStatus>(status, true);
            allBookings = allBookings.Where(b => b.Status == bookingStatus);
        }

        return allBookings
            .OrderByDescending(b => b.BookingDate)
            .ThenBy(b => b.StartTime)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToList();
    }

    public async Task<Booking?> GetBookingByIdAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var booking = await _bookingRepo.GetByIdAsync(id);
        return booking?.CondominiumId == tenantId ? booking : null;
    }

    public async Task<List<Booking>> GetCalendarAsync(Guid? serviceId, DateOnly startDate, DateOnly endDate)
    {
        var tenantId = GetCurrentTenantId();
        var allBookings = (await _bookingRepo.GetAllAsync())
            .Where(b => b.CondominiumId == tenantId
                && b.BookingDate >= startDate
                && b.BookingDate <= endDate
                && b.Status != BookingStatus.Cancelled
                && b.Status != BookingStatus.Rejected);

        if (serviceId.HasValue)
            allBookings = allBookings.Where(b => b.ServiceId == serviceId.Value);

        return allBookings
            .OrderBy(b => b.BookingDate)
            .ThenBy(b => b.StartTime)
            .ToList();
    }

    public async Task<Booking> CreateBookingAsync(
        Guid serviceId,
        Guid unitId,
        DateOnly bookingDate,
        TimeOnly startTime,
        TimeOnly endTime,
        string? title = null,
        string? description = null,
        int guestsCount = 0,
        string? specialRequirements = null)
    {
        var tenantId = GetCurrentTenantId();

        var allServices = await _serviceRepo.GetAllAsync();
        var service = allServices.FirstOrDefault(s => s.Id == serviceId && s.CondominiumId == tenantId);
        if (service == null)
            throw new InvalidOperationException("Serviço não encontrado");
        if (!service.IsActive)
            throw new InvalidOperationException("Serviço não está disponível");

        var allBookings = await _bookingRepo.GetAllAsync();
        var hasConflict = allBookings.Any(b =>
            b.ServiceId == serviceId
            && b.BookingDate == bookingDate
            && b.Status != BookingStatus.Cancelled
            && b.Status != BookingStatus.Rejected
            && startTime < b.EndTime
            && endTime > b.StartTime);

        if (hasConflict)
            throw new InvalidOperationException("Já existe uma reserva neste horário");

        if (service.MaxBookingPerDay.HasValue)
        {
            var bookingsToday = allBookings.Count(b =>
                b.ServiceId == serviceId
                && b.BookingDate == bookingDate
                && b.Status != BookingStatus.Cancelled
                && b.Status != BookingStatus.Rejected);

            if (bookingsToday >= service.MaxBookingPerDay.Value)
                throw new InvalidOperationException($"Limite de {service.MaxBookingPerDay} reservas por dia atingido");
        }

        var allResidents = await _residentRepo.GetAllAsync();
        var resident = allResidents.FirstOrDefault(r =>
            r.UnitId == unitId && r.CondominiumId == tenantId && r.IsActive);
        if (resident == null)
            throw new InvalidOperationException("Morador não encontrado para esta unidade");

        var booking = Booking.Create(
            tenantId,
            serviceId,
            unitId,
            resident.Id,
            bookingDate,
            startTime,
            endTime,
            service.RequiresApproval,
            amount: service.Price > 0 ? service.Price : null,
            title: title,
            description: description,
            guestsCount: guestsCount);

        await _bookingRepo.AddAsync(booking);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Reserva criada: {BookingId} para serviço {ServiceId}", booking.Id, serviceId);

        return booking;
    }

    public async Task<Booking?> ApproveBookingAsync(Guid id, Guid approvedBy)
    {
        var tenantId = GetCurrentTenantId();
        var booking = await _bookingRepo.GetByIdAsync(id);
        if (booking == null || booking.CondominiumId != tenantId) return null;

        booking.Approve(approvedBy);
        _bookingRepo.Update(booking);
        await _unitOfWork.SaveChangesAsync();

        return booking;
    }

    public async Task<Booking?> RejectBookingAsync(Guid id, string reason)
    {
        var tenantId = GetCurrentTenantId();
        var booking = await _bookingRepo.GetByIdAsync(id);
        if (booking == null || booking.CondominiumId != tenantId) return null;

        booking.Reject(Guid.Empty, reason);
        _bookingRepo.Update(booking);
        await _unitOfWork.SaveChangesAsync();

        return booking;
    }

    public async Task<Booking?> CancelBookingAsync(Guid id, Guid cancelledBy, string reason)
    {
        var tenantId = GetCurrentTenantId();
        var booking = await _bookingRepo.GetByIdAsync(id);
        if (booking == null || booking.CondominiumId != tenantId) return null;

        booking.Cancel(cancelledBy, reason);
        _bookingRepo.Update(booking);
        await _unitOfWork.SaveChangesAsync();

        return booking;
    }

    public async Task<Booking?> CheckInAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var booking = await _bookingRepo.GetByIdAsync(id);
        if (booking == null || booking.CondominiumId != tenantId) return null;

        booking.CheckIn();
        _bookingRepo.Update(booking);
        await _unitOfWork.SaveChangesAsync();

        return booking;
    }

    public async Task<Booking?> CheckOutAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var booking = await _bookingRepo.GetByIdAsync(id);
        if (booking == null || booking.CondominiumId != tenantId) return null;

        booking.CheckOut();
        _bookingRepo.Update(booking);
        await _unitOfWork.SaveChangesAsync();

        return booking;
    }
}