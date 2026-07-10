using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondoSync.Application.Features.Bookings.DTOs;
using CondoSync.Application.Services;

namespace CondoSync.Api.Controllers;

[Authorize(AuthenticationSchemes = "Tenant")]
public class BookingsController : BaseController
{
    private readonly BookingService _bookingService;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(BookingService bookingService, ILogger<BookingsController> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    /// <summary>
    /// Lista reservas com filtros
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? serviceId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 20)
    {
        var bookings = await _bookingService.GetBookingsAsync(serviceId, startDate, endDate, status, page, perPage);

        var response = bookings.Select(b => new BookingResponse(
            b.Id, b.ServiceId, "", b.UnitId, "",
            b.ResidentId, "", b.BookingDate,
            b.StartTime.ToString(), b.EndTime.ToString(),
            b.Status.ToString(), b.Title, b.GuestsCount,
            b.Amount, b.PaymentStatus?.ToString(), b.CreatedAt
        ));

        return Ok(new { success = true, data = response, meta = new { page, perPage } });
    }

    /// <summary>
    /// Obtém calendário de reservas
    /// </summary>
    [HttpGet("calendar")]
    public async Task<IActionResult> GetCalendar(
        [FromQuery] Guid? serviceId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var start = startDate.HasValue ? DateOnly.FromDateTime(startDate.Value)
            : DateOnly.FromDateTime(DateTime.UtcNow);
        var end = endDate.HasValue ? DateOnly.FromDateTime(endDate.Value)
            : start.AddMonths(1);

        var bookings = await _bookingService.GetCalendarAsync(serviceId, start, end);

        var response = bookings.Select(b => new
        {
            b.Id,
            b.ServiceId,
            b.BookingDate,
            b.StartTime,
            b.EndTime,
            b.Status,
            b.Title
        });

        return Ok(new { success = true, data = response });
    }

    /// <summary>
    /// Obtém detalhes de uma reserva
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);

        if (booking == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Reserva não encontrada" } });

        return Ok(new { success = true, data = booking });
    }

    /// <summary>
    /// Cria uma nova reserva
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
    {
        try
        {
            var booking = await _bookingService.CreateBookingAsync(
                request.ServiceId,
                request.UnitId,
                DateOnly.FromDateTime(request.BookingDate),
                TimeOnly.Parse(request.StartTime),
                TimeOnly.Parse(request.EndTime),
                request.Title,
                request.Description,
                request.GuestsCount,
                request.SpecialRequirements);

            return CreatedAtAction(nameof(GetById), new { id = booking.Id }, new
            {
                success = true,
                data = new { booking.Id, booking.BookingDate, booking.Status }
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = new { code = "VALIDATION_ERROR", message = ex.Message } });
        }
    }

    /// <summary>
    /// Aprova uma reserva pendente
    /// </summary>
    [HttpPatch("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var booking = await _bookingService.ApproveBookingAsync(id, userId.Value);

        if (booking == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Reserva não encontrada" } });

        return Ok(new { success = true, message = "Reserva aprovada com sucesso" });
    }

    /// <summary>
    /// Rejeita uma reserva pendente
    /// </summary>
    [HttpPatch("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] ApproveRejectRequest request)
    {
        var booking = await _bookingService.RejectBookingAsync(id, request.Reason ?? "Não especificado");

        if (booking == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Reserva não encontrada" } });

        return Ok(new { success = true, message = "Reserva rejeitada" });
    }

    /// <summary>
    /// Cancela uma reserva
    /// </summary>
    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelBookingRequest request)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var booking = await _bookingService.CancelBookingAsync(id, userId.Value, request.Reason);

        if (booking == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Reserva não encontrada" } });

        return Ok(new { success = true, message = "Reserva cancelada com sucesso" });
    }

    /// <summary>
    /// Realiza check-in em uma reserva
    /// </summary>
    [HttpPatch("{id:guid}/checkin")]
    public async Task<IActionResult> CheckIn(Guid id)
    {
        var booking = await _bookingService.CheckInAsync(id);

        if (booking == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Reserva não encontrada" } });

        return Ok(new { success = true, message = "Check-in realizado com sucesso" });
    }

    /// <summary>
    /// Realiza check-out em uma reserva
    /// </summary>
    [HttpPatch("{id:guid}/checkout")]
    public async Task<IActionResult> CheckOut(Guid id)
    {
        var booking = await _bookingService.CheckOutAsync(id);

        if (booking == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Reserva não encontrada" } });

        return Ok(new { success = true, message = "Check-out realizado com sucesso" });
    }
}