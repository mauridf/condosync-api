using CondoSync.Core.Interfaces;

namespace CondoSync.Infrastructure.Tenant;

public class TenantProvider : ITenantProvider
{
    private static readonly AsyncLocal<Guid?> _currentTenantId = new();
    private static readonly AsyncLocal<string?> _currentTenantSlug = new();

    public Guid? GetCurrentTenantId()
    {
        return _currentTenantId.Value;
    }

    public string? GetCurrentTenantSlug()
    {
        return _currentTenantSlug.Value;
    }

    public void SetCurrentTenant(Guid tenantId, string slug)
    {
        _currentTenantId.Value = tenantId;
        _currentTenantSlug.Value = slug;
    }
}