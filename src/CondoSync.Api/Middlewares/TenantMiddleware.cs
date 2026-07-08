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
        // Skippar rotas do admin (domínio global)
        if (context.Request.Path.StartsWithSegments("/api/v1/admin"))
        {
            await _next(context);
            return;
        }

        // TODO: Implementar resolução de tenant
        // Por enquanto, apenas passa adiante
        _logger.LogDebug("TenantMiddleware: Path {Path} - Tenant não implementado ainda",
            context.Request.Path);

        await _next(context);
    }
}