using Microsoft.Extensions.Configuration;
using smart_doorbell.Application.DTOs;
using smart_doorbell.Application.Interfaces;
using smart_doorbell.Application.Interfaces.Repositories;
using smart_doorbell.Application.Interfaces.Services;
using smart_doorbell.Domain.Enums;

namespace smart_doorbell.Application.Services;

public class CameraBrokerService : ICameraBrokerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMinioStorageService _minioService;
    private readonly IConfiguration _configuration;

    public CameraBrokerService(IUnitOfWork unitOfWork, IMinioStorageService minioService, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _minioService = minioService;
        _configuration = configuration;
    }

    public async Task<CameraUploadCredentialsResponse> GetUploadCredentialsAsync(
        Guid cameraDeviceId,
        string authToken,
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        var device = await _unitOfWork.Devices.GetByIdAsync(cameraDeviceId, cancellationToken);
        if (device == null || device.Type != DeviceType.Camera)
        {
            throw new KeyNotFoundException("Camera device not found.");
        }

        if (device.AuthToken != authToken)
        {
            throw new UnauthorizedAccessException("Invalid camera device authentication token.");
        }

        var bucket = _configuration["MinioOptions:BucketName"] ?? "doorbell-snapshots";
        await _minioService.EnsureBucketExistsAsync(bucket, cancellationToken);

        var uploadUrl = await _minioService.GetPresignedPutUrlAsync(objectKey, bucket, 900, cancellationToken);

        return new CameraUploadCredentialsResponse(uploadUrl, objectKey, bucket);
    }
}
