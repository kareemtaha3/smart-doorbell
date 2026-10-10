using smart_doorbell.Application.DTOs;
using smart_doorbell.Application.Interfaces;
using smart_doorbell.Application.Interfaces.Repositories;
using smart_doorbell.Domain.Entities;

namespace smart_doorbell.Application.Services;

public class PushSubscriptionService : IPushSubscriptionService
{
    private readonly IUnitOfWork _unitOfWork;

    public PushSubscriptionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PushSubscriptionResponseDto> SubscribeAsync(Guid userId, SubscribePushDto dto)
    {
        var existing = await _unitOfWork.PushSubscriptions.GetByUserIdAndEndpointAsync(userId, dto.Endpoint);

        if (existing != null)
        {
            existing.P256dhKey = dto.P256dhKey;
            existing.AuthKey = dto.AuthKey;
            existing.LastUsedAt = DateTime.UtcNow;
        }
        else
        {
            existing = new PushSubscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Endpoint = dto.Endpoint,
                P256dhKey = dto.P256dhKey,
                AuthKey = dto.AuthKey,
                CreatedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow
            };
            await _unitOfWork.PushSubscriptions.AddAsync(existing);
        }

        await _unitOfWork.SaveChangesAsync();

        return new PushSubscriptionResponseDto(
            existing.Id,
            existing.UserId,
            existing.Endpoint,
            existing.CreatedAt,
            existing.LastUsedAt
        );
    }

    public async Task UnsubscribeAsync(Guid userId, string endpoint)
    {
        var existing = await _unitOfWork.PushSubscriptions.GetByUserIdAndEndpointAsync(userId, endpoint);

        if (existing != null)
        {
            _unitOfWork.PushSubscriptions.Remove(existing);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<PushSubscriptionResponseDto>> GetUserSubscriptionsAsync(Guid userId)
    {
        var subs = await _unitOfWork.PushSubscriptions.GetByUserIdAsync(userId);

        return subs.Select(s => new PushSubscriptionResponseDto(
            s.Id,
            s.UserId,
            s.Endpoint,
            s.CreatedAt,
            s.LastUsedAt
        ));
    }
}
