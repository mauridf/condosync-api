using ClosedXML.Excel;
using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CondoSync.Application.Services;

public class ReportService
{
    private readonly IRepository<Bill> _billRepository;
    private readonly IRepository<Booking> _bookingRepository;
    private readonly IRepository<Resident> _residentRepository;
    private readonly IRepository<ActivityLog> _activityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        IRepository<Bill> billRepository,
        IRepository<Booking> bookingRepository,
        IRepository<Resident> residentRepository,
        IRepository<ActivityLog> activityRepository,
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        ILogger<ReportService> logger)
    {
        _billRepository = billRepository;
        _bookingRepository = bookingRepository;
        _residentRepository = residentRepository;
        _activityRepository = activityRepository;
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantProvider.GetCurrentTenantId()
            ?? throw new UnauthorizedAccessException("Tenant não identificado");
    }

    private const string TenantName = "CondoSync";

    private void InitPdf() => QuestPDF.Settings.License = LicenseType.Community;

    // ─── BILLS ───────────────────────────────────────────────────────────

    public async Task<byte[]> GenerateBillsPdfAsync(DateOnly? startDate, DateOnly? endDate, string? status)
    {
        var bills = await GetFilteredBillsAsync(startDate, endDate, status);
        InitPdf();
        return QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(15);
                page.DefaultTextStyle(x => x.FontSize(9));
                page.Header().AlignCenter().Text("Relatório de Faturas").FontSize(14).Bold();
                page.Content().Table(t =>
                {
                    t.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2); c.RelativeColumn(4);
                        c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(2);
                    });
                    t.Header(h =>
                    {
                        h.Cell().Text("Nº").Bold(); h.Cell().Text("Descrição").Bold();
                        h.Cell().Text("Vencimento").Bold(); h.Cell().Text("Valor").Bold(); h.Cell().Text("Status").Bold();
                    });
                    foreach (var b in bills)
                    {
                        t.Cell().Text(b.BillNumber ?? "-");
                        t.Cell().Text(b.Description);
                        t.Cell().Text(b.DueDate.ToString("dd/MM/yyyy"));
                        t.Cell().Text(b.TotalAmount.ToString("C"));
                        t.Cell().Text(b.Status.ToString());
                    }
                });
                page.Footer().AlignRight().Text(x => { x.Span("Página "); x.CurrentPageNumber(); });
            });
        }).GeneratePdf();
    }

    public async Task<byte[]> GenerateBillsExcelAsync(DateOnly? startDate, DateOnly? endDate, string? status)
    {
        var bills = await GetFilteredBillsAsync(startDate, endDate, status);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Faturas");
        ws.Cell(1, 1).Value = "Número"; ws.Cell(1, 2).Value = "Descrição";
        ws.Cell(1, 3).Value = "Vencimento"; ws.Cell(1, 4).Value = "Valor Base";
        ws.Cell(1, 5).Value = "Desconto"; ws.Cell(1, 6).Value = "Multa";
        ws.Cell(1, 7).Value = "Juros"; ws.Cell(1, 8).Value = "Total";
        ws.Cell(1, 9).Value = "Status"; ws.Cell(1, 10).Value = "Pagamento";
        ws.Cell(1, 11).Value = "Método";
        for (int i = 0; i < bills.Count; i++)
        {
            var b = bills[i];
            var r = i + 2;
            ws.Cell(r, 1).Value = b.BillNumber ?? "";
            ws.Cell(r, 2).Value = b.Description;
            ws.Cell(r, 3).Value = b.DueDate.ToString("dd/MM/yyyy");
            ws.Cell(r, 4).Value = (double)b.BaseAmount;
            ws.Cell(r, 5).Value = (double)b.DiscountAmount;
            ws.Cell(r, 6).Value = (double)b.FineAmount;
            ws.Cell(r, 7).Value = (double)b.InterestAmount;
            ws.Cell(r, 8).Value = (double)b.TotalAmount;
            ws.Cell(r, 9).Value = b.Status.ToString();
            ws.Cell(r, 10).Value = b.PaymentDate?.ToString("dd/MM/yyyy") ?? "";
            ws.Cell(r, 11).Value = b.PaymentMethod ?? "";
        }
        ws.Columns().AdjustToContents();
        return SaveToBytes(wb);
    }

    // ─── BOOKINGS ────────────────────────────────────────────────────────

    public async Task<byte[]> GenerateBookingsPdfAsync(DateOnly? startDate, DateOnly? endDate, string? status)
    {
        var bookings = await GetFilteredBookingsAsync(startDate, endDate, status);
        InitPdf();
        return QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(15);
                page.DefaultTextStyle(x => x.FontSize(9));
                page.Header().AlignCenter().Text("Relatório de Reservas").FontSize(14).Bold();
                page.Content().Table(t =>
                {
                    t.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3); c.RelativeColumn(2);
                        c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(1);
                    });
                    t.Header(h =>
                    {
                        h.Cell().Text("Título").Bold(); h.Cell().Text("Data").Bold();
                        h.Cell().Text("Horário").Bold(); h.Cell().Text("Status").Bold(); h.Cell().Text("Convidados").Bold();
                    });
                    foreach (var b in bookings)
                    {
                        t.Cell().Text(b.Title ?? "-");
                        t.Cell().Text(b.BookingDate.ToString("dd/MM/yyyy"));
                        t.Cell().Text($"{b.StartTime:hh\\:mm}-{b.EndTime:hh\\:mm}");
                        t.Cell().Text(b.Status.ToString());
                        t.Cell().Text(b.GuestsCount.ToString());
                    }
                });
                page.Footer().AlignRight().Text(x => { x.Span("Página "); x.CurrentPageNumber(); });
            });
        }).GeneratePdf();
    }

    public async Task<byte[]> GenerateBookingsExcelAsync(DateOnly? startDate, DateOnly? endDate, string? status)
    {
        var bookings = await GetFilteredBookingsAsync(startDate, endDate, status);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Reservas");
        ws.Cell(1, 1).Value = "Título"; ws.Cell(1, 2).Value = "Data";
        ws.Cell(1, 3).Value = "Início"; ws.Cell(1, 4).Value = "Fim";
        ws.Cell(1, 5).Value = "Status"; ws.Cell(1, 6).Value = "Convidados";
        ws.Cell(1, 7).Value = "Valor"; ws.Cell(1, 8).Value = "Pagamento";
        for (int i = 0; i < bookings.Count; i++)
        {
            var b = bookings[i];
            var r = i + 2;
            ws.Cell(r, 1).Value = b.Title ?? "";
            ws.Cell(r, 2).Value = b.BookingDate.ToString("dd/MM/yyyy");
            ws.Cell(r, 3).Value = b.StartTime.ToString();
            ws.Cell(r, 4).Value = b.EndTime.ToString();
            ws.Cell(r, 5).Value = b.Status.ToString();
            ws.Cell(r, 6).Value = b.GuestsCount;
            ws.Cell(r, 7).Value = b.Amount.HasValue ? (double)b.Amount.Value : 0;
            ws.Cell(r, 8).Value = b.PaymentStatus?.ToString() ?? "";
        }
        ws.Columns().AdjustToContents();
        return SaveToBytes(wb);
    }

    // ─── RESIDENTS ───────────────────────────────────────────────────────

    public async Task<byte[]> GenerateResidentsPdfAsync()
    {
        var residents = await GetResidentsAsync();
        InitPdf();
        return QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(15);
                page.DefaultTextStyle(x => x.FontSize(9));
                page.Header().AlignCenter().Text("Relatório de Moradores").FontSize(14).Bold();
                page.Content().Table(t =>
                {
                    t.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3); c.RelativeColumn(2);
                        c.RelativeColumn(3); c.RelativeColumn(2); c.RelativeColumn(1);
                    });
                    t.Header(h =>
                    {
                        h.Cell().Text("Nome").Bold(); h.Cell().Text("Tipo").Bold();
                        h.Cell().Text("Email").Bold(); h.Cell().Text("Telefone").Bold(); h.Cell().Text("Ativo").Bold();
                    });
                    foreach (var r in residents)
                    {
                        t.Cell().Text(r.Name);
                        t.Cell().Text(r.ResidentType.ToString());
                        t.Cell().Text(r.Email ?? "-");
                        t.Cell().Text(r.Phone ?? "-");
                        t.Cell().Text(r.IsActive ? "Sim" : "Não");
                    }
                });
                page.Footer().AlignRight().Text(x => { x.Span("Página "); x.CurrentPageNumber(); });
            });
        }).GeneratePdf();
    }

    public async Task<byte[]> GenerateResidentsExcelAsync()
    {
        var residents = await GetResidentsAsync();
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Moradores");
        ws.Cell(1, 1).Value = "Nome"; ws.Cell(1, 2).Value = "Tipo";
        ws.Cell(1, 3).Value = "Email"; ws.Cell(1, 4).Value = "Telefone";
        ws.Cell(1, 5).Value = "CPF"; ws.Cell(1, 6).Value = "Ativo";
        ws.Cell(1, 7).Value = "Principal"; ws.Cell(1, 8).Value = "Entrada";
        for (int i = 0; i < residents.Count; i++)
        {
            var r = residents[i];
            var row = i + 2;
            ws.Cell(row, 1).Value = r.Name;
            ws.Cell(row, 2).Value = r.ResidentType.ToString();
            ws.Cell(row, 3).Value = r.Email ?? "";
            ws.Cell(row, 4).Value = r.Phone ?? "";
            ws.Cell(row, 5).Value = r.Cpf ?? "";
            ws.Cell(row, 6).Value = r.IsActive ? "Sim" : "Não";
            ws.Cell(row, 7).Value = r.IsPrimary ? "Sim" : "Não";
            ws.Cell(row, 8).Value = r.MoveInDate?.ToString("dd/MM/yyyy") ?? "";
        }
        ws.Columns().AdjustToContents();
        return SaveToBytes(wb);
    }

    // ─── ACTIVITY ────────────────────────────────────────────────────────

    public async Task<byte[]> GenerateActivityPdfAsync(DateTime? startDate, DateTime? endDate, string? action = null)
    {
        var activities = await GetFilteredActivitiesAsync(startDate, endDate, action);
        InitPdf();
        return QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(15);
                page.DefaultTextStyle(x => x.FontSize(9));
                page.Header().AlignCenter().Text("Relatório de Atividades").FontSize(14).Bold();
                page.Content().Table(t =>
                {
                    t.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3); c.RelativeColumn(3);
                        c.RelativeColumn(2); c.RelativeColumn(4);
                    });
                    t.Header(h =>
                    {
                        h.Cell().Text("Data").Bold(); h.Cell().Text("Ação").Bold();
                        h.Cell().Text("Entidade").Bold(); h.Cell().Text("Detalhes").Bold();
                    });
                    foreach (var a in activities)
                    {
                        t.Cell().Text(a.CreatedAt.ToString("dd/MM/yyyy HH:mm"));
                        t.Cell().Text(a.Action);
                        t.Cell().Text(a.EntityType);
                        t.Cell().Text(a.Details ?? "-");
                    }
                });
                page.Footer().AlignRight().Text(x => { x.Span("Página "); x.CurrentPageNumber(); });
            });
        }).GeneratePdf();
    }

    public async Task<byte[]> GenerateActivityExcelAsync(DateTime? startDate, DateTime? endDate, string? action = null)
    {
        var activities = await GetFilteredActivitiesAsync(startDate, endDate, action);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Atividades");
        ws.Cell(1, 1).Value = "Data"; ws.Cell(1, 2).Value = "Ação";
        ws.Cell(1, 3).Value = "Entidade"; ws.Cell(1, 4).Value = "Detalhes";
        ws.Cell(1, 5).Value = "Usuário";
        for (int i = 0; i < activities.Count; i++)
        {
            var a = activities[i];
            var r = i + 2;
            ws.Cell(r, 1).Value = a.CreatedAt.ToString("dd/MM/yyyy HH:mm");
            ws.Cell(r, 2).Value = a.Action;
            ws.Cell(r, 3).Value = a.EntityType;
            ws.Cell(r, 4).Value = a.Details ?? "";
            ws.Cell(r, 5).Value = a.UserRole ?? "";
        }
        ws.Columns().AdjustToContents();
        return SaveToBytes(wb);
    }

    // ─── HELPERS ─────────────────────────────────────────────────────────

    private async Task<List<Bill>> GetFilteredBillsAsync(DateOnly? startDate, DateOnly? endDate, string? status)
    {
        var tenantId = GetCurrentTenantId();
        var bills = await _billRepository.FindAsync(b => b.CondominiumId == tenantId);
        var query = bills.AsQueryable();
        if (startDate.HasValue) query = query.Where(b => b.DueDate >= startDate.Value);
        if (endDate.HasValue) query = query.Where(b => b.DueDate <= endDate.Value);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(b => b.Status == Enum.Parse<BillStatus>(status, true));
        return query.OrderBy(b => b.DueDate).ToList();
    }

    private async Task<List<Booking>> GetFilteredBookingsAsync(DateOnly? startDate, DateOnly? endDate, string? status)
    {
        var tenantId = GetCurrentTenantId();
        var bookings = await _bookingRepository.FindAsync(b => b.CondominiumId == tenantId);
        var query = bookings.AsQueryable();
        if (startDate.HasValue) query = query.Where(b => b.BookingDate >= startDate.Value);
        if (endDate.HasValue) query = query.Where(b => b.BookingDate <= endDate.Value);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(b => b.Status == Enum.Parse<BookingStatus>(status, true));
        return query.OrderBy(b => b.BookingDate).ToList();
    }

    private async Task<List<Resident>> GetResidentsAsync()
    {
        var tenantId = GetCurrentTenantId();
        var residents = await _residentRepository.FindAsync(r => r.CondominiumId == tenantId);
        return residents.OrderBy(r => r.Name).ToList();
    }

    private async Task<List<ActivityLog>> GetFilteredActivitiesAsync(DateTime? startDate, DateTime? endDate, string? action)
    {
        var tenantId = GetCurrentTenantId();
        var activities = await _activityRepository.FindAsync(a => a.CondominiumId == tenantId);
        var query = activities.AsQueryable();
        if (startDate.HasValue) query = query.Where(a => a.CreatedAt >= startDate.Value);
        if (endDate.HasValue) query = query.Where(a => a.CreatedAt <= endDate.Value);
        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action == action);
        return query.OrderByDescending(a => a.CreatedAt).ToList();
    }

    private static byte[] SaveToBytes(XLWorkbook wb)
    {
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
