using CondoSync.Api.Extensions;
using CondoSync.Api.Middlewares;
using FluentValidation;
using Scalar.AspNetCore;
using Serilog;
using System.Text.Json;
using System.Text.Json.Serialization;

// Configurar Serilog como primeiro passo
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ==========================================
    // 1. Configurar Serilog
    // ==========================================
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.WithProperty("Application", "CondoSync")
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
            .Enrich.FromLogContext();
    });

    // ==========================================
    // 2. Configurar Services
    // ==========================================

    // Configurar JSON
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

    // Documentação Swagger/Scalar
    builder.Services.AddCondoSyncSwagger();

    // Autenticação JWT dupla
    builder.Services.AddCondoSyncAuthentication(builder.Configuration);

    // Rate Limiting
    builder.Services.AddCondoSyncRateLimiting();

    // CORS
    builder.Services.AddCondoSyncCors(builder.Configuration);

    // Health Checks
    builder.Services.AddCondoSyncHealthChecks(builder.Configuration);

    // AutoMapper
    builder.Services.AddAutoMapper(typeof(CondoSync.Application.Common.Mappings.CondominiumProfile).Assembly);

    // MediatR
    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(CondoSync.Application.Common.DTOs.PaginatedResult<>).Assembly);
    });

    // FluentValidation
    builder.Services.AddValidatorsFromAssembly(typeof(CondoSync.Application.Common.DTOs.ErrorResponse).Assembly);

    // Configurar porta
    builder.WebHost.UseUrls("http://localhost:5000");

    var app = builder.Build();

    // ==========================================
    // 3. Configurar Pipeline HTTP
    // ==========================================

    // Middleware de exceção global (primeiro na pipeline)
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // Serilog request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    // Swagger e Scalar (disponível em todos os ambientes para dev)
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CondoSync API v1");
        c.RoutePrefix = "swagger";
    });

    // Scalar para documentação interativa
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("CondoSync API")
            .WithTheme(Scalar.AspNetCore.ScalarTheme.Purple)
            .WithDefaultHttpClient(Scalar.AspNetCore.ScalarTarget.CSharp, Scalar.AspNetCore.ScalarClient.HttpClient);
    });

    // Redirecionar root para Scalar
    app.MapGet("/", () => Results.Redirect("/scalar/v1"));

    // CORS
    if (app.Environment.IsDevelopment())
    {
        app.UseCors("Development");
    }
    else
    {
        app.UseCors("Production");
    }

    // Rate Limiting
    app.UseRateLimiter();

    // Autenticação e Autorização
    app.UseAuthentication();
    app.UseAuthorization();

    // Health Checks
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var response = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description
                }),
                duration = report.TotalDuration
            };
            await context.Response.WriteAsJsonAsync(response);
        }
    });

    // Mapear controllers
    app.MapControllers();

    // ==========================================
    // 4. Iniciar aplicação
    // ==========================================

    Log.Information("🚀 CondoSync API iniciando no ambiente {Environment}...",
        app.Environment.EnvironmentName);

    Log.Information("📚 Swagger disponível em: http://localhost:5000/swagger");
    Log.Information("📖 Scalar disponível em: http://localhost:5000/scalar/v1");
    Log.Information("❤️ Health check em: http://localhost:5000/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Aplicação terminou inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}