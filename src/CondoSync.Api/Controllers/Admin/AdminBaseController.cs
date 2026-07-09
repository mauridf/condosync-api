using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CondoSync.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/[controller]")]
[Authorize(AuthenticationSchemes = "Admin", Roles = "super_admin,support,analyst")]
public abstract class AdminBaseController : ControllerBase
{
    protected Guid CurrentAdminId
    {
        get
        {
            var adminIdClaim = User.FindFirst("adminId")?.Value;
            if (Guid.TryParse(adminIdClaim, out var adminId))
                return adminId;

            throw new UnauthorizedAccessException("Admin não identificado");
        }
    }

    protected string CurrentRole
    {
        get
        {
            return User.FindFirst("role")?.Value
                ?? throw new UnauthorizedAccessException("Role não identificada");
        }
    }
}