using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace CondoSync.Infrastructure.External.Notification;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    Task SendToManyAsync(IEnumerable<string> recipients, string subject, string body, CancellationToken cancellationToken = default);
}

public class SmtpEmailService : IEmailService
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    private readonly string _fromAddress;
    private readonly bool _useSsl;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(string host, int port, string username, string password,
        string fromAddress, bool useSsl, ILogger<SmtpEmailService> logger)
    {
        _host = host;
        _port = port;
        _username = username;
        _password = password;
        _fromAddress = fromAddress;
        _useSsl = useSsl;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        using var message = CreateMessage(to, subject, body);

        await client.SendMailAsync(message, cancellationToken);
        _logger.LogDebug("Email enviado para {To}: {Subject}", to, subject);
    }

    public async Task SendToManyAsync(IEnumerable<string> recipients, string subject, string body, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        var toList = recipients.ToList();

        foreach (var to in toList)
        {
            using var message = CreateMessage(to, subject, body);
            await client.SendMailAsync(message, cancellationToken);
        }

        _logger.LogInformation("Email enviado para {Count} destinatários: {Subject}", toList.Count, subject);
    }

    private SmtpClient CreateClient()
    {
        return new SmtpClient(_host, _port)
        {
            Credentials = new NetworkCredential(_username, _password),
            EnableSsl = _useSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
        };
    }

    private MailMessage CreateMessage(string to, string subject, string body)
    {
        return new MailMessage
        {
            From = new MailAddress(_fromAddress),
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
        }.WithTo(to);
    }
}

internal static class MailMessageExtensions
{
    public static MailMessage WithTo(this MailMessage message, string to)
    {
        message.To.Add(to);
        return message;
    }
}
