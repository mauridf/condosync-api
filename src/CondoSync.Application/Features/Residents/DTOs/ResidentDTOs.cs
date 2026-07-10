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
    string? OwnerName = null,
    string? OwnerPhone = null,
    string? OwnerEmail = null,
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
    Guid? Id,
    string Plate,
    string Model,
    string? Color = null,
    string? Brand = null
);

public record PetDTO(
    Guid? Id,
    string Name,
    string Species,
    string? Breed = null,
    string? Color = null
);

public record AddVehicleRequest(string Plate, string Model, string? Color = null, string? Brand = null);

public record UpdateVehicleRequest(string Plate, string Model, string? Color = null, string? Brand = null);

public record AddPetRequest(string Name, string Species, string? Breed = null, string? Color = null);

public record UpdatePetRequest(string Name, string Species, string? Breed = null, string? Color = null);

public record VehicleResponse(Guid Id, string Plate, string Model, string? Color, string? Brand);

public record PetResponse(Guid Id, string Name, string Species, string? Breed, string? Color);

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
    List<VehicleResponse>? Vehicles,
    List<PetResponse>? Pets,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record ToggleAccessRequest(
    bool GrantAccess
);

public record UpdateResidentRoleRequest(
    string Role
);
