namespace CondoSync.Application.Features.Bills.DTOs;

public record GenerateBillRequest(
    Guid UnitId,
    string Description,
    string ReferenceMonth,
    decimal BaseAmount,
    DateTime DueDate,
    decimal? LateFeePercentage = null,
    decimal? LateInterestDaily = null
);

public record BatchGenerateBillsRequest(
    string ReferenceMonth,
    string Description,
    DateTime DueDate,
    List<Guid>? UnitIds = null  // null = todas as unidades
);

public record PayBillRequest(
    decimal Amount,
    string PaymentMethod,
    string TransactionId
);

public record InstallmentRequest(
    int NumberOfInstallments,
    DateTime FirstDueDate
);

public record BillResponse(
    Guid Id,
    Guid UnitId,
    string UnitNumber,
    string? BillNumber,
    string Description,
    string ReferenceMonth,
    decimal BaseAmount,
    decimal TotalAmount,
    decimal? FineAmount,
    decimal? InterestAmount,
    DateTime DueDate,
    string Status,
    DateTime? PaymentDate,
    decimal? PaymentAmount,
    DateTime CreatedAt
);

public record BillDetailResponse(
    Guid Id,
    Guid CondominiumId,
    Guid UnitId,
    string? BillNumber,
    string Description,
    string ReferenceMonth,
    decimal BaseAmount,
    decimal DiscountAmount,
    string? DiscountType,
    decimal FineAmount,
    decimal InterestAmount,
    decimal TotalAmount,
    decimal? Balance,
    DateOnly IssueDate,
    DateOnly DueDate,
    DateOnly? FineStartDate,
    decimal LateFeePercentage,
    decimal LateInterestDaily,
    string Status,
    DateOnly? PaymentDate,
    decimal? PaymentAmount,
    string? PaymentMethod,
    string? TransactionId,
    int InstallmentNumber,
    int TotalInstallments,
    string? BoletoUrl,
    string? BoletoCode,
    string? PixCode,
    string? PixQrCodeUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record OverdueReportResponse(
    Guid BillId,
    Guid UnitId,
    string UnitNumber,
    string? BillNumber,
    string Description,
    string ReferenceMonth,
    decimal TotalAmount,
    DateTime DueDate,
    int DaysOverdue,
    decimal FineAmount,
    decimal InterestAmount,
    string Status
);

public record BillSummaryResponse(
    string ReferenceMonth,
    int TotalBills,
    int PaidBills,
    int PendingBills,
    int OverdueBills,
    decimal TotalAmount,
    decimal TotalPaid,
    decimal TotalPending
);