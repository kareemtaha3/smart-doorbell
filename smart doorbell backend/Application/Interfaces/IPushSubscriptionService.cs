using smart_doorbell.Application.DTOs;

namespace smart_doorbell.Application.Interfaces;

public interface IPushSubscriptionService
{
    Task<PushSubscriptionResponseDto> SubscribeAsync(Guid userId, SubscribePushDto dto);
    Task UnsubscribeAsync(Guid userId, string endpoint);
    Task<IEnumerable<PushSubscriptionResponseDto>> GetUserSubscriptionsAsync(Guid userId);
}
