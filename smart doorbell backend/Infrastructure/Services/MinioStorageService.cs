using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using smart_doorbell.Application.Interfaces;

namespace smart_doorbell.Infrastructure.Services;

public class MinioStorageService : IMinioStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MinioStorageService> _logger;

    public MinioStorageService(
        IMinioClient minioClient,
        IConfiguration configuration,
        ILogger<MinioStorageService> logger)
    {
        _minioClient = minioClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task EnsureBucketExistsAsync(string bucket, CancellationToken cancellationToken = default)
    {
        try
        {
            var beArgs = new BucketExistsArgs().WithBucket(bucket);
            bool found = await _minioClient.BucketExistsAsync(beArgs, cancellationToken);
            if (!found)
            {
                var mbArgs = new MakeBucketArgs().WithBucket(bucket);
                await _minioClient.MakeBucketAsync(mbArgs, cancellationToken);
                _logger.LogInformation("[MinIO] Bucket '{Bucket}' did not exist and was created successfully on startup.", bucket);
            }
            else
            {
                _logger.LogInformation("[MinIO] Bucket '{Bucket}' verified and ready.", bucket);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[MinIO Warning] Could not ensure bucket '{Bucket}' exists (MinIO service may be starting or unreachable).", bucket);
        }
    }

    public async Task<string> GetPresignedObjectUrlAsync(string objectKey, string bucket, CancellationToken cancellationToken = default)
    {
        try
        {
            var args = new PresignedGetObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectKey)
                .WithExpiry(60 * 60 * 24); 

            var url = await _minioClient.PresignedGetObjectAsync(args);
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MinIO Error] Error generating presigned GET URL for '{ObjectKey}' in '{Bucket}'", objectKey, bucket);
            var endpoint = _configuration["MinioOptions:Endpoint"] ?? "localhost:9000";
            return $"http://{endpoint}/{bucket}/{objectKey}";
        }
    }

    public async Task<string> GetPresignedPutUrlAsync(string objectKey, string bucket, int expirySeconds = 900, CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureBucketExistsAsync(bucket, cancellationToken);

            var args = new PresignedPutObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectKey)
                .WithExpiry(expirySeconds);

            var url = await _minioClient.PresignedPutObjectAsync(args);
            _logger.LogInformation("[MinIO] Presigned PUT Upload URL generated for object '{ObjectKey}' in bucket '{Bucket}'", objectKey, bucket);
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MinIO Error] Error generating presigned PUT URL for '{ObjectKey}' in '{Bucket}'", objectKey, bucket);
            var endpoint = _configuration["MinioOptions:Endpoint"] ?? "localhost:9000";
            return $"http://{endpoint}/{bucket}/{objectKey}?uploadToken={Guid.NewGuid()}";
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string objectKey, string bucket, string contentType, CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureBucketExistsAsync(bucket, cancellationToken);

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectKey)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);
            _logger.LogInformation("[MinIO] File '{ObjectKey}' uploaded to bucket '{Bucket}'", objectKey, bucket);

            return await GetPresignedObjectUrlAsync(objectKey, bucket, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MinIO Error] Failed to upload object '{ObjectKey}' to bucket '{Bucket}'", objectKey, bucket);
            var endpoint = _configuration["MinioOptions:Endpoint"] ?? "localhost:9000";
            return $"http://{endpoint}/{bucket}/{objectKey}";
        }
    }
}
