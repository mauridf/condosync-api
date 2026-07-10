namespace CondoSync.Application.Features.Employees.DTOs;

public record CreateEmployeeRequest(
    string Name,
    string Email,
    string Password,
    string? Phone = null,
    string? Document = null,
    string? Role = "Employee",
    Guid? UnitId = null
);

public record EmployeeResponse(
    Guid Id,
    Guid UserId,
    string Name,
    string Email,
    string? Phone,
    string? Document,
    string Role,
    bool IsActive,
    DateTime CreatedAt
);
