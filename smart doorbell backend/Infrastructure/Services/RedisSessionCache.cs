using Microsoft.Extensions.Logging;
using smart_doorbell.Application.Interfaces;
using StackExchange.Redis;

namespace smart_doorbell.Infrastructure.Services;

public class RedisSessionCache : IRedisSessionCache
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisSessionCache> _logger;

    public RedisSessionCache(IConnectionMultiplexer redis, ILogger<RedisSessionCache> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    private static string GetKey(Guid buttonDeviceId) => $"doorbell:session:{buttonDeviceId}";

    public async Task<Guid?> GetActiveSessionIdAsync(Guid buttonDeviceId, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = GetKey(buttonDeviceId);
        var val = await db.StringGetAsync(key);

        if (!val.HasValue || !Guid.TryParse(val.ToString(), out var sessionId))
        {
            return null;
        }

        return sessionId;
    }

    public async Task SetActiveSessionAsync(Guid buttonDeviceId, Guid sessionId, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = GetKey(buttonDeviceId);
        await db.StringSetAsync(key, sessionId.ToString(), expiry);
        _logger.LogInformation("Redis session set for device {DeviceId} -> session {SessionId} (TTL: {Expiry}s)", buttonDeviceId, sessionId, expiry.TotalSeconds);
    }

    public async Task RefreshSilenceTimerAsync(Guid buttonDeviceId, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = GetKey(buttonDeviceId);
        var refreshed = await db.KeyExpireAsync(key, expiry);
        if (refreshed)
        {
            _logger.LogInformation("Redis silence timer refreshed for device {DeviceId} (New TTL: {Expiry}s)", buttonDeviceId, expiry.TotalSeconds);
        }
    }

    public async Task RemoveSessionAsync(Guid buttonDeviceId, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = GetKey(buttonDeviceId);
        await db.KeyDeleteAsync(key);
        _logger.LogInformation("Redis session key deleted for device {DeviceId}", buttonDeviceId);
    }
}
