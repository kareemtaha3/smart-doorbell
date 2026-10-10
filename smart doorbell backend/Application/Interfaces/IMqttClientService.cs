namespace smart_doorbell.Application.Interfaces;

public interface IMqttClientService
{
    Task PublishChimeCommandAsync(Guid chimeDeviceId, Guid melodyId, Guid ringEventId, string? toneFileUrl, CancellationToken cancellationToken = default);
    Task PublishCameraSnapshotCommandAsync(Guid cameraDeviceId, Guid sessionId, string objectKey, CancellationToken cancellationToken = default);
    Task ProcessChimeAckAsync(Guid chimeDeviceId, Guid ringEventId, CancellationToken cancellationToken = default);
    Task ProcessCameraAckAsync(Guid cameraDeviceId, Guid sessionId, CancellationToken cancellationToken = default);
}
