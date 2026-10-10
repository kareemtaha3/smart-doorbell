namespace smart_doorbell.Application.Interfaces;

public interface IRedisSessionCache
{
    Task<Guid?> GetActiveSessionIdAsync(Guid buttonDeviceId, CancellationToken cancellationToken = default);
    Task SetActiveSessionAsync(Guid buttonDeviceId, Guid sessionId, TimeSpan expiry, CancellationToken cancellationToken = default);
    Task RefreshSilenceTimerAsync(Guid buttonDeviceId, TimeSpan expiry, CancellationToken cancellationToken = default);
    Task RemoveSessionAsync(Guid buttonDeviceId, CancellationToken cancellationToken = default);
}
