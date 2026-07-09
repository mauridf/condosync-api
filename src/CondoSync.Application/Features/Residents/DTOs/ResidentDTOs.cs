using CondoSync.Core.Enums;

namespace CondoSync.Application.Features.Residents.DTOs;

public record CreateResidentRequest(
    Guid UnitId,
    string Name,
    string ResidentType = "owner",
    string? Email = null,
    string? Phone = null,
    string? Cpf = null,
    string? Rg = null,
    DateOnly? BirthDate = null,
    string? Profession = null,
    bool IsPrimary = false,
    bool IsEmergencyContact = false,
    // Proprietário (se locatário)
    string? OwnerName = null,
    string? OwnerPhone = null,
    string? OwnerEmail = null,
    // Veículos e Pets
    List<VehicleDTO>? Vehicles = null,
    List<PetDTO>? Pets = null
);

public record UpdateResidentRequest(
    string Name,
    string? Email = null,
    string? Phone = null,
    string? Profession = null,
    bool? IsEmergencyContact = null
);

public record VehicleDTO(
    string Plate,
    string Model,
    string? Color = null,
    string? Brand = null
);

public record PetDTO(
    string Name,
    string Species,
    string? Breed = null,
    string? Color = null
);

public record ResidentResponse(
    Guid Id,
    Guid UnitId,
    string? UserId,
    string Name,
    string ResidentType,
    string? Email,
    string? Phone,
    string? Cpf,
    bool IsPrimary,
    bool IsActive,
    bool HasSystemAccess,
    DateOnly? MoveInDate,
    DateTime CreatedAt
);

public record ResidentDetailResponse(
    Guid Id,
    Guid CondominiumId,
    Guid UnitId,
    Guid? UserId,
    string Name,
    string ResidentType,
    string? Email,
    string? Phone,
    string? Cpf,
    string? Rg,
    DateOnly? BirthDate,
    string? Profession,
    bool IsPrimary,
    bool IsActive,
    bool IsEmergencyContact,
    bool HasSystemAccess,
    string? OwnerName,
    string? OwnerPhone,
    string? OwnerEmail,
    DateOnly? MoveInDate,
    DateOnly? MoveOutDate,
    List<VehicleDTO>? Vehicles,
    List<PetDTO>? Pets,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record ToggleAccessRequest(
    bool GrantAccess
);