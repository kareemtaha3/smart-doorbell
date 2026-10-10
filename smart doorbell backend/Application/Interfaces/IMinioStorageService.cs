namespace smart_doorbell.Application.Interfaces;

public interface IMinioStorageService
{
    Task<string> GetPresignedObjectUrlAsync(string objectKey, string bucket, CancellationToken cancellationToken = default);
    Task<string> GetPresignedPutUrlAsync(string objectKey, string bucket, int expirySeconds = 900, CancellationToken cancellationToken = default);
    Task<string> UploadFileAsync(Stream fileStream, string objectKey, string bucket, string contentType, CancellationToken cancellationToken = default);
    Task EnsureBucketExistsAsync(string bucket, CancellationToken cancellationToken = default);
}
