using Microsoft.Extensions.Logging;

namespace CondoSync.Infrastructure.External.Notification;

public interface IPushNotificationService
{
    Task SendAsync(string deviceToken, string title, string body, Dictionary<string, string>? data = null, CancellationToken cancellationToken = default);
    Task SendToManyAsync(IEnumerable<string> deviceTokens, string title, string body, Dictionary<string, string>? data = null, CancellationToken cancellationToken = default);
}

public class StubPushNotificationService : IPushNotificationService
{
    private readonly ILogger<StubPushNotificationService> _logger;

    public StubPushNotificationService(ILogger<StubPushNotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string deviceToken, string title, string body, Dictionary<string, string>? data = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[PUSH STUB] Para {DeviceToken}: {Title} - {Body}", deviceToken, title, body);
        return Task.CompletedTask;
    }

    public Task SendToManyAsync(IEnumerable<string> deviceTokens, string title, string body, Dictionary<string, string>? data = null, CancellationToken cancellationToken = default)
    {
        var tokens = deviceTokens.ToList();
        _logger.LogInformation("[PUSH STUB] Para {Count} dispositivos: {Title} - {Body}", tokens.Count, title, body);
        return Task.CompletedTask;
    }
}
