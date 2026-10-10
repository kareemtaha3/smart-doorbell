using smart_doorbell.Application.DTOs;

namespace smart_doorbell.Application.Interfaces.Services;

public interface ICameraBrokerService
{
    Task<CameraUploadCredentialsResponse> GetUploadCredentialsAsync(Guid cameraDeviceId, string authToken, string objectKey, CancellationToken cancellationToken = default);
}
