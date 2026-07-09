using CondoSync.Core.Enums;

namespace CondoSync.Application.Features.Units.DTOs;

public record CreateUnitRequest(
    string Number,
    string Type = "apartment",
    string? Block = null,
    string? Floor = null,
    decimal? Area = null,
    int Bedrooms = 0,
    int Bathrooms = 0,
    int ParkingSpots = 0,
    decimal? MonthlyFee = null
);

public record UpdateUnitRequest(
    string Number,
    string Type,
    string? Block = null,
    string? Floor = null,
    decimal? Area = null,
    int? Bedrooms = null,
    int? Bathrooms = null,
    int? ParkingSpots = null,
    decimal? MonthlyFee = null
);

public record BatchCreateUnitsRequest(
    List<CreateUnitRequest> Units
);

public record UnitResponse(
    Guid Id,
    string? Block,
    string Number,
    string? Floor,
    string Type,
    decimal? Area,
    int Bedrooms,
    int Bathrooms,
    int ParkingSpots,
    string OccupancyStatus,
    bool IsActive,
    decimal? MonthlyFee,
    DateTime CreatedAt
);

public record UnitDetailResponse(
    Guid Id,
    Guid CondominiumId,
    string? Block,
    string Number,
    string? Floor,
    string Type,
    decimal? Area,
    int Bedrooms,
    int Bathrooms,
    int ParkingSpots,
    bool IsActive,
    bool IsRented,
    string OccupancyStatus,
    decimal? MonthlyFee,
    decimal LateFeePercentage,
    decimal InterestPercentage,
    decimal? IptuAnnual,
    DateTime CreatedAt,
    DateTime UpdatedAt
);