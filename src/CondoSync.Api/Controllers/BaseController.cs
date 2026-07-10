using Microsoft.AspNetCore.Mvc;
using CondoSync.Core.Interfaces;

namespace CondoSync.Api.Controllers;

[ApiController]
[Route("api/v1/{slug}/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected Guid CurrentTenantId
    {
        get
        {
            if (HttpContext.Items["TenantId"] is Guid tenantId)
                return tenantId;

            throw new UnauthorizedAccessException("Tenant não identificado");
        }
    }

    protected string CurrentTenantSlug
    {
        get
        {
            if (HttpContext.Items["TenantSlug"] is string slug)
                return slug;

            throw new UnauthorizedAccessException("Tenant não identificado");
        }
    }

    protected Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }

    protected Guid? GetTenantId()
    {
        var tenantIdClaim = User.FindFirst("tenantId")?.Value;
        if (Guid.TryParse(tenantIdClaim, out var tenantId))
            return tenantId;
        return null;
    }

    protected string? GetUserRole()
    {
        return User.FindFirst("role")?.Value;
    }
}