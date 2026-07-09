using CondoSync.Core.Entities;
using CondoSync.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CondoSync.Infrastructure.Tenant;

public class TenantInterceptor : SaveChangesInterceptor
{
    private readonly ITenantProvider _tenantProvider;

    public TenantInterceptor(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is null)
            return base.SavingChanges(eventData, result);

        var tenantId = _tenantProvider.GetCurrentTenantId();

        if (!tenantId.HasValue)
            return base.SavingChanges(eventData, result);

        // Preencher automaticamente CondominiumId em novas entidades
        foreach (var entry in eventData.Context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added && e.Entity is ITenantEntity))
        {
            var entity = (ITenantEntity)entry.Entity;
            var property = entry.Property(nameof(entity.CondominiumId));

            if (property.CurrentValue is Guid guid && guid == Guid.Empty)
            {
                property.CurrentValue = tenantId.Value;
            }
        }

        return base.SavingChanges(eventData, result);
    }
}