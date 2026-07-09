using CondoSync.Api.Extensions;
using CondoSync.Api.Middlewares;
using CondoSync.Infrastructure;
using CondoSync.Infrastructure.Data.Migrations;
using CondoSync.Application;
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

    // Infrastructure (DbContexts, Repositories, UnitOfWork)
    builder.Services.AddInfrastructure(builder.Configuration);

    // Application
    builder.Services.AddApplication();

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

    // Middleware de Tenant (primeiro na pipeline, antes da autenticação)
    app.UseMiddleware<TenantMiddleware>();
    // Middleware de exceção global (primeiro na pipeline)
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // Serilog request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    // Swagger (disponível em /swagger)
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "swagger/{documentName}/swagger.json";
    });

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CondoSync API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "CondoSync API - Swagger";
    });

    // Scalar (disponível em /scalar/v1)
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("CondoSync API")
            .WithTheme(ScalarTheme.Purple)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
            .WithPreferredScheme("Bearer")
            .WithDarkModeToggle(true)
            .WithOpenApiRoutePattern("/swagger/v1/swagger.json");
    });

    // Redirecionamentos
    app.MapGet("/", () => Results.Redirect("/scalar/v1"));
    app.MapGet("/docs", () => Results.Redirect("/scalar/v1"));
    app.MapGet("/api", () => Results.Redirect("/scalar/v1"));

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
                timestamp = DateTime.UtcNow,
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description
                }),
                totalDuration = report.TotalDuration.ToString()
            };
            await context.Response.WriteAsJsonAsync(response);
        }
    });

    // Health check rápido (liveness)
    app.MapGet("/healthz", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

    // ==========================================
    // 4. Executar Migrations via DbUp
    // ==========================================
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;

        try
        {
            var migrator = services.GetRequiredService<DatabaseMigrator>();
            migrator.RunMigrations();

            Log.Information("✅ Banco de dados pronto");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "❌ Erro crítico ao executar migrations");
            throw;
        }
    }

    // Mapear controllers
    app.MapControllers();

    // ==========================================
    // 5. Iniciar aplicação
    // ==========================================

    Log.Information("🚀 CondoSync API iniciando no ambiente {Environment}...",
        app.Environment.EnvironmentName);
    Log.Information("📖 Scalar (Principal): http://localhost:5000/scalar/v1");
    Log.Information("📚 Swagger: http://localhost:5000/swagger");
    Log.Information("❤️ Health Check: http://localhost:5000/health");
    Log.Information("💚 Liveness: http://localhost:5000/healthz");

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