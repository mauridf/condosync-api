using System.Net;
using System.Text.Json;
using CondoSync.Core.Exceptions;

namespace CondoSync.Api.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação: {Message}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var response = new
            {
                success = false,
                error = new
                {
                    code = "VALIDATION_ERROR",
                    message = ex.Message,
                    details = ex.Errors.Select(e => new { field = e.Field, message = e.Message })
                },
                meta = new { requestId = context.TraceIdentifier, timestamp = DateTime.UtcNow }
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Recurso não encontrado");
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "application/json";

            var response = new
            {
                success = false,
                error = new { code = "NOT_FOUND", message = ex.Message },
                meta = new { requestId = context.TraceIdentifier, timestamp = DateTime.UtcNow }
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning(ex, "Conflito: {Message}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            context.Response.ContentType = "application/json";

            var response = new
            {
                success = false,
                error = new { code = "CONFLICT", message = ex.Message },
                meta = new { requestId = context.TraceIdentifier, timestamp = DateTime.UtcNow }
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Erro de domínio: {Message}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            context.Response.ContentType = "application/json";

            var response = new
            {
                success = false,
                error = new { code = ex.ErrorCode, message = ex.Message },
                meta = new { requestId = context.TraceIdentifier, timestamp = DateTime.UtcNow }
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado: {Message}", ex.Message);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new
            {
                success = false,
                error = new
                {
                    code = "INTERNAL_ERROR",
                    message = "Ocorreu um erro interno. Tente novamente mais tarde."
                },
                meta = new
                {
                    requestId = context.TraceIdentifier,
                    timestamp = DateTime.UtcNow
                }
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}