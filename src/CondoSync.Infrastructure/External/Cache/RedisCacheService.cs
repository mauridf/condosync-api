using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace CondoSync.Infrastructure.External.Cache;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly ConnectionMultiplexer _connection;

    public RedisCacheService(string connectionString, ILogger<RedisCacheService> logger)
    {
        _connection = ConnectionMultiplexer.Connect(connectionString);
        _database = _connection.GetDatabase();
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var value = await _database.StringGetAsync(key);
            if (!value.HasValue) return null;

            return JsonSerializer.Deserialize<T>((string)value!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter cache para chave {CacheKey}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var serialized = JsonSerializer.Serialize(value);
            if (expiration.HasValue)
                await _database.StringSetAsync(key, serialized, expiration.Value);
            else
                await _database.StringSetAsync(key, serialized);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao definir cache para chave {CacheKey}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover cache para chave {CacheKey}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _database.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar cache para chave {CacheKey}", key);
            return false;
        }
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
