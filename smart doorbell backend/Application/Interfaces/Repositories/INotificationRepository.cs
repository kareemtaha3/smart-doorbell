using smart_doorbell.Domain.Entities;

namespace smart_doorbell.Application.Interfaces.Repositories;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Notification?> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
}
