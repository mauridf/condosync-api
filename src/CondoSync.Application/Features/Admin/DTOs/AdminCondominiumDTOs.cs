using CondoSync.Core.Enums;

namespace CondoSync.Application.Features.Admin.DTOs;

public record CondominiumListResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Email,
    string SubscriptionPlan,
    string SubscriptionStatus,
    bool IsActive,
    DateTime CreatedAt
);

public record CondominiumDetailResponse(
    Guid Id,
    string Name,
    string? Cnpj,
    string Slug,
    string? Address,
    string? City,
    string? State,
    string? ZipCode,
    string? Phone,
    string? Email,
    string? LogoUrl,
    string SubscriptionPlan,
    string SubscriptionStatus,
    DateTime? SubscriptionExpiresAt,
    DateTime? TrialEndsAt,
    int MaxUnits,
    int MaxResidentsPerUnit,
    string Timezone,
    string Language,
    List<string> EnabledModules,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record UpdateCondominiumRequest(
    string Name,
    string? Email = null,
    string? Phone = null,
    string? Address = null,
    string? City = null,
    string? State = null,
    string? ZipCode = null
);

public record ChangePlanRequest(
    string Plan  // trial, free, basic, premium, enterprise
);

public record CreateCondominiumRequest(
    string Name,
    string Slug,
    string AdminName,
    string AdminEmail,
    string AdminPassword,
    string? Cnpj = null,
    string? Phone = null,
    string Plan = "trial"
);