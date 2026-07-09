namespace CondoSync.Application.Common.Interfaces;

public interface ICurrentTenantService
{
    Guid? TenantId { get; }
    string? TenantSlug { get; }
    bool IsAuthenticated { get; }
}