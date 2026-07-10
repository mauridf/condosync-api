using CondoSync.Core.Enums;

namespace CondoSync.Core.Entities;

public class Unit : AggregateRoot<Guid>, ITenantEntity
{
    public Guid CondominiumId { get; private set; }

    // Identificação
    public string? Block { get; private set; }
    public string Number { get; private set; } = default!;
    public string? Floor { get; private set; }
    public UnitType Type { get; private set; }

    // Características
    public decimal? Area { get; private set; }
    public int Bedrooms { get; private set; }
    public int Bathrooms { get; private set; }
    public int ParkingSpots { get; private set; }

    // Status
    public bool IsActive { get; private set; }
    public bool IsRented { get; private set; }
    public UnitOccupancyStatus OccupancyStatus { get; private set; }

    // Financeiro
    public decimal? MonthlyFee { get; private set; }
    public decimal LateFeePercentage { get; private set; }
    public decimal InterestPercentage { get; private set; }
    public decimal? IptuAnnual { get; private set; }

    // Metadados
    public string? CustomFields { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private Unit() { }

    public static Unit Create(
        Guid condominiumId,
        string number,
        UnitType type = UnitType.Apartment,
        string? block = null,
        string? floor = null,
        decimal? area = null,
        int bedrooms = 0,
        int bathrooms = 0,
        int parkingSpots = 0,
        decimal? monthlyFee = null)
    {
        return new Unit
        {
            Id = Guid.NewGuid(),
            CondominiumId = condominiumId,
            Number = number,
            Type = type,
            Block = block,
            Floor = floor,
            Area = area,
            Bedrooms = bedrooms,
            Bathrooms = bathrooms,
            ParkingSpots = parkingSpots,
            MonthlyFee = monthlyFee,
            LateFeePercentage = 2.00m,
            InterestPercentage = 0.033m,
            OccupancyStatus = UnitOccupancyStatus.Vacant,
            IsActive = true,
            IsRented = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string number,
        UnitType type,
        string? block = null,
        string? floor = null,
        decimal? area = null,
        int? bedrooms = null,
        int? bathrooms = null,
        int? parkingSpots = null)
    {
        Number = number;
        Type = type;
        if (block != null) Block = block;
        if (floor != null) Floor = floor;
        if (area.HasValue) Area = area.Value;
        if (bedrooms.HasValue) Bedrooms = bedrooms.Value;
        if (bathrooms.HasValue) Bathrooms = bathrooms.Value;
        if (parkingSpots.HasValue) ParkingSpots = parkingSpots.Value;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateFinancialInfo(decimal? monthlyFee, decimal? lateFeePercentage = null,
        decimal? interestPercentage = null, decimal? iptuAnnual = null)
    {
        if (monthlyFee.HasValue) MonthlyFee = monthlyFee.Value;
        if (lateFeePercentage.HasValue) LateFeePercentage = lateFeePercentage.Value;
        if (interestPercentage.HasValue) InterestPercentage = interestPercentage.Value;
        if (iptuAnnual.HasValue) IptuAnnual = iptuAnnual.Value;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsOccupied(UnitOccupancyStatus status)
    {
        OccupancyStatus = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsVacant()
    {
        OccupancyStatus = UnitOccupancyStatus.Vacant;
        IsRented = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsRented()
    {
        IsRented = true;
        OccupancyStatus = UnitOccupancyStatus.OccupiedByTenant;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}