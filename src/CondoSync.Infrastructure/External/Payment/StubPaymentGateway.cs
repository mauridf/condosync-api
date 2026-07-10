using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CondoSync.Infrastructure.External.Payment;

public class StubPaymentGateway : IPaymentGateway
{
    private readonly ILogger<StubPaymentGateway> _logger;
    private static readonly Random _random = new();
    private static int _counter;

    public StubPaymentGateway(ILogger<StubPaymentGateway> logger)
    {
        _logger = logger;
    }

    public async Task<string> GenerateBoletoAsync(decimal amount, string description, DateTime dueDate,
        Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
    {
        var id = Interlocked.Increment(ref _counter);
        var boletoCode = $"34191.{_random.Next(10000, 99999)} {_random.Next(10000, 99999)} " +
                         $"{_random.Next(10000, 99999)} {_random.Next(10000, 99999)} " +
                         $"{_random.Next(10000, 99999)} {_random.Next(1000, 9999)} " +
                         $"{_random.Next(100000, 999999)}";

        _logger.LogInformation("[STUB] Boleto generated #{Id}: {Code}", id, boletoCode[..20]);

        await Task.Delay(100, cancellationToken);

        return boletoCode;
    }

    public async Task<string> GeneratePixAsync(decimal amount, string description,
        Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
    {
        var id = Interlocked.Increment(ref _counter);
        var pixCode = $"00020126580014br.gov.bcb.pix0136stub-condosync-{id:X8}" +
                      $"520400005303986540{amount:F2}5802BR5913CondoSync6008BRASILIA" +
                      $"62070503***6304{_random.Next(1000, 9999)}";

        _logger.LogInformation("[STUB] PIX generated #{Id}: {Code}", id, pixCode[..30]);

        await Task.Delay(100, cancellationToken);

        return pixCode;
    }

    public async Task<bool> ProcessPaymentAsync(string transactionId, PaymentMethod method,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(200, cancellationToken);

        // Simula 95% de sucesso
        var success = _random.NextDouble() < 0.95;
        _logger.LogInformation("[STUB] Payment {TransactionId} processed via {Method}: {Result}",
            transactionId, method, success ? "SUCCESS" : "FAILED");

        return success;
    }
}
