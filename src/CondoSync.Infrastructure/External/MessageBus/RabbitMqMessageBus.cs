using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace CondoSync.Infrastructure.External.MessageBus;

public class RabbitMqMessageBus : IMessageBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqMessageBus> _logger;
    private bool _disposed;

    public RabbitMqMessageBus(string host, int port, string username, string password, ILogger<RabbitMqMessageBus> logger)
    {
        var factory = new ConnectionFactory
        {
            HostName = host,
            Port = port,
            UserName = username,
            Password = password,
        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _logger = logger;
    }

    public async Task PublishAsync<T>(T message, string routingKey = "", CancellationToken cancellationToken = default) where T : class
    {
        var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
        try
        {
            var exchangeName = typeof(T).Name;
            await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            var properties = new BasicProperties
            {
                Persistent = true,
                Type = typeof(T).AssemblyQualifiedName,
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await channel.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogDebug("Mensagem publicada no exchange {Exchange}, routing key {RoutingKey}",
                exchangeName, routingKey);
        }
        finally
        {
            await channel.CloseAsync(cancellationToken: cancellationToken);
        }
    }

    public async Task SubscribeAsync<T>(string queueName, Func<T, Task> handler, CancellationToken cancellationToken = default) where T : class
    {
        var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
        var exchangeName = typeof(T).Name;

        await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(queueName, exchangeName, routingKey: "#", cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, args) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(args.Body.Span);
                var message = JsonSerializer.Deserialize<T>(body);

                if (message != null)
                {
                    await handler(message);
                    await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem do tipo {MessageType} na fila {QueueName}",
                    typeof(T).Name, queueName);
                await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: true, cancellationToken: cancellationToken);
            }
        };

        await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);

        _logger.LogInformation("Assinante registrado para fila {QueueName}, exchange {Exchange}",
            queueName, exchangeName);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _connection?.CloseAsync().GetAwaiter().GetResult();
            _connection?.Dispose();
            _disposed = true;
        }
    }
}
