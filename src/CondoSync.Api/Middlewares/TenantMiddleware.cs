using Microsoft.EntityFrameworkCore;
using CondoSync.Core.Interfaces;
using CondoSync.Infrastructure.Data;

namespace CondoSync.Api.Middlewares;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // SKIP para rotas do admin (domínio global - sem tenant)
        if (context.Request.Path.StartsWithSegments("/api/v1/admin"))
        {
            _logger.LogDebug("Admin route - skipping tenant resolution");
            await _next(context);
            return;
        }

        // SKIP para rotas públicas (health, swagger, scalar, auth pública)
        if (IsPublicRoute(context.Request.Path))
        {
            _logger.LogDebug("Public route {Path} - skipping tenant resolution", context.Request.Path);
            await _next(context);
            return;
        }

        // Resolver tenant pelo slug
        var tenantSlug = ResolveTenantSlug(context);

        if (string.IsNullOrEmpty(tenantSlug))
        {
            _logger.LogWarning("Tenant slug não encontrado na requisição {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                "{\"success\":false,\"error\":{\"code\":\"TENANT_REQUIRED\",\"message\":\"Identificador do condomínio é obrigatório\"}}");
            return;
        }

        // Buscar tenant no banco
        try
        {
            var dbContext = context.RequestServices.GetRequiredService<CondoSyncDbContext>();
            var tenant = await dbContext.Condominiums
                .FirstOrDefaultAsync(c => c.Slug == tenantSlug && c.IsActive);

            if (tenant == null)
            {
                _logger.LogWarning("Tenant não encontrado: {Slug}", tenantSlug);
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    "{\"success\":false,\"error\":{\"code\":\"TENANT_NOT_FOUND\",\"message\":\"Condomínio não encontrado\"}}");
                return;
            }

            // Verificar se tenant está ativo
            if (!tenant.IsActive)
            {
                _logger.LogWarning("Tenant inativo: {Slug}", tenantSlug);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    "{\"success\":false,\"error\":{\"code\":\"TENANT_INACTIVE\",\"message\":\"Condomínio suspenso ou cancelado\"}}");
                return;
            }

            // Definir tenant no contexto
            var tenantProvider = context.RequestServices.GetRequiredService<ITenantProvider>();
            tenantProvider.SetCurrentTenant(tenant.Id, tenantSlug);

            // Armazenar no HttpContext para acesso fácil
            context.Items["TenantId"] = tenant.Id;
            context.Items["TenantSlug"] = tenantSlug;
            context.Items["Tenant"] = tenant;

            _logger.LogDebug("Tenant resolvido: {Slug} ({TenantId})", tenantSlug, tenant.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao resolver tenant: {Slug}", tenantSlug);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                "{\"success\":false,\"error\":{\"code\":\"INTERNAL_ERROR\",\"message\":\"Erro interno ao processar requisição\"}}");
            return;
        }

        await _next(context);
    }

    private static string? ResolveTenantSlug(HttpContext context)
    {
        // 1. Slug na rota: /api/v1/{slug}/...
        var path = context.Request.Path.Value;
        if (!string.IsNullOrEmpty(path))
        {
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            // /api/v1/{slug}/...
            if (segments.Length >= 3 && segments[0] == "api" && segments[1] == "v1")
            {
                // Pular "api", "v1", pegar o terceiro segmento
                // Mas só se não for um endpoint conhecido
                var knownEndpoints = new[] { "auth", "health", "swagger", "scalar" };
                if (!knownEndpoints.Contains(segments[2].ToLower()))
                {
                    return segments[2];
                }
            }
        }

        // 2. Header X-Tenant-Slug
        var headerSlug = context.Request.Headers["X-Tenant-Slug"].FirstOrDefault();
        if (!string.IsNullOrEmpty(headerSlug))
            return headerSlug;

        // 3. Subdomínio (para futuro uso com custom domains)
        var host = context.Request.Host.Host;
        if (host.Contains('.'))
        {
            var subdomain = host.Split('.')[0];
            if (subdomain != "www" && subdomain != "api" && subdomain != "app")
                return subdomain;
        }

        return null;
    }

    private static bool IsPublicRoute(string path)
    {
        var publicPaths = new[]
        {
            "/health",
            "/healthz",
            "/swagger",
            "/scalar",
            "/api/v1/auth/register",
            "/api/v1/auth/login",
            "/api/v1/auth/refresh",
            "/api/v1/auth/forgot-password",
            "/api/v1/auth/reset-password",
            "/api/v1/auth/verify-email",
            "/api/v1/admin/auth/login",
            "/api/v1/admin/auth/refresh",
            "/api/v1/admin/auth/forgot-password",
            "/api/v1/admin/auth/reset-password"
        };

        return publicPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
    }
}