using CondoSync.Core.Entities;
using CondoSync.Core.Interfaces;
using CondoSync.Infrastructure.External.Notification;
using System.Text.Json;

namespace CondoSync.Api.BackgroundServices;

public class NotificationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationWorker> _logger;

    public NotificationWorker(IServiceScopeFactory scopeFactory, ILogger<NotificationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificationWorker iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingNotifications(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no ciclo de processamento de notificações");
            }

            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }

    private async Task ProcessPendingNotifications(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var notificationRepo = scope.ServiceProvider.GetRequiredService<IRepository<Notification>>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var pushService = scope.ServiceProvider.GetRequiredService<IPushNotificationService>();
        var userRepo = scope.ServiceProvider.GetRequiredService<IRepository<User>>();

        var pending = await notificationRepo.FindAsync(n =>
            n.SentAt == null && n.Channels != "[\"in_app\"]");

        foreach (var notification in pending)
        {
            try
            {
                var channels = notification.Channels is not null
                    ? JsonSerializer.Deserialize<List<string>>(notification.Channels) ?? []
                    : [];

                if (channels.Contains("email"))
                {
                    var users = await userRepo.FindAsync(u => u.Id == notification.UserId);
                    var user = users.FirstOrDefault();
                    if (user?.Email != null)
                    {
                        await emailService.SendAsync(user.Email, notification.Title, notification.Body ?? "", ct);
                    }
                }

                if (channels.Contains("push"))
                {
                    await pushService.SendAsync(
                        notification.UserId.ToString(),
                        notification.Title,
                        notification.Body ?? "",
                        new Dictionary<string, string>
                        {
                            ["entityType"] = notification.EntityType ?? "",
                            ["entityId"] = notification.EntityId?.ToString() ?? "",
                            ["action"] = notification.Action ?? ""
                        }, ct);
                }

                notification.Send();
                _logger.LogDebug("Notificação {NotificationId} processada via {Channels}",
                    notification.Id, notification.Channels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao processar notificação {NotificationId}", notification.Id);
            }
        }

        if (pending.Any())
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
    }
}
