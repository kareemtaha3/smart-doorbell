namespace smart_doorbell.Application.DTOs;

public record MelodyResponseDto(
    Guid Id,
    Guid HomeId,
    string Name,
    string ToneFileUrl,
    bool IsMain,
    DateTime CreatedAt
);

public record CameraUploadCredentialsRequest(
    Guid CameraDeviceId,
    string AuthToken,
    string ObjectKey
);

public record CameraUploadCredentialsResponse(
    string UploadUrl,
    string ObjectKey,
    string Bucket
);
