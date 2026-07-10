using CondoSync.Application.Common.Interfaces;
using CondoSync.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CondoSync.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Serviços de autenticação
        services.AddScoped<IPasswordHasher, PasswordService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<AuthService>();

        // Serviços administrativos
        services.AddScoped<AdminService>();
        services.AddScoped<UnitService>();
        services.AddScoped<ResidentService>();
        services.AddScoped<ServiceManagementService>();
        services.AddScoped<BookingService>();
        services.AddScoped<BillService>();
        services.AddScoped<VisitorService>();
        services.AddScoped<TicketService>();
        services.AddScoped<NoticeService>();
        services.AddScoped<PollService>();
        services.AddScoped<CondoDashboardService>();
        services.AddScoped<DocumentService>();
        services.AddScoped<NotificationTemplateService>();
        services.AddScoped<CommonAreaService>();
        services.AddScoped<UnitInvitationService>();
        services.AddScoped<ReportService>();

        return services;
    }
}