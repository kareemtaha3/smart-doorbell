using smart_doorbell.Application.DTOs;

namespace smart_doorbell.Application.Interfaces.Services;

public interface IDoorbellService
{
    Task<RingSessionDetailDto> ProcessRingEventAsync(Guid buttonDeviceId, string deviceEventId, CancellationToken cancellationToken = default);
    Task CloseSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task ProcessMinioUploadWebhookAsync(string objectKey, string bucket, CancellationToken cancellationToken = default);
}
