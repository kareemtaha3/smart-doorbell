using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using smart_doorbell.Application.Interfaces.Services;

namespace smart_doorbell.Presentation.Controllers;

[ApiController]
[Route("api/webhook/minio")]
public class MinioWebhookController : ControllerBase
{
    private readonly IDoorbellService _doorbellService;
    private readonly ILogger<MinioWebhookController> _logger;
    private readonly IConfiguration _configuration;

    public MinioWebhookController(
        IDoorbellService doorbellService,
        ILogger<MinioWebhookController> logger,
        IConfiguration configuration)
    {
        _doorbellService = doorbellService;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> HandleMinioWebhook([FromBody] JsonElement payload, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[MinIO Webhook] Received webhook payload: {RawJson}", payload.GetRawText());

        string? bucket = null;
        string? objectKey = null;

        // 1. Check standard MinIO / S3 Event Notification Records array
        if (payload.TryGetProperty("Records", out var records) && records.ValueKind == JsonValueKind.Array && records.GetArrayLength() > 0)
        {
            var firstRecord = records[0];
            if (firstRecord.TryGetProperty("s3", out var s3))
            {
                if (s3.TryGetProperty("bucket", out var b) && b.TryGetProperty("name", out var bName))
                {
                    bucket = bName.GetString();
                }

                if (s3.TryGetProperty("object", out var o) && o.TryGetProperty("key", out var oKey))
                {
                    objectKey = oKey.GetString();
                }
            }
        }

        // 2. Fallback to top-level Key / objectKey / ObjectKey properties
        if (string.IsNullOrEmpty(objectKey) && payload.TryGetProperty("Key", out var topKey))
        {
            objectKey = topKey.GetString();
        }
        if (string.IsNullOrEmpty(objectKey) && payload.TryGetProperty("objectKey", out var optKey))
        {
            objectKey = optKey.GetString();
        }
        if (string.IsNullOrEmpty(objectKey) && payload.TryGetProperty("ObjectKey", out var optKey2))
        {
            objectKey = optKey2.GetString();
        }

        // Fallback for Bucket property
        if (string.IsNullOrEmpty(bucket) && payload.TryGetProperty("Bucket", out var topBucket))
        {
            bucket = topBucket.GetString();
        }
        if (string.IsNullOrEmpty(bucket) && payload.TryGetProperty("bucket", out var optBucket))
        {
            bucket = optBucket.GetString();
        }

        // 3. Fallback to configured bucket if bucket is omitted in payload
        if (string.IsNullOrEmpty(bucket))
        {
            bucket = _configuration["MinioOptions:BucketName"] ?? "doorbell-snapshots";
        }

        if (string.IsNullOrEmpty(objectKey))
        {
            _logger.LogWarning("[MinIO Webhook] Webhook payload did not contain a valid ObjectKey or Key.");
            return BadRequest(new { message = "ObjectKey or Key is required in webhook payload." });
        }

        // Decode URL encoding (e.g. MinIO encodes slashes as %2F)
        var decodedObjectKey = WebUtility.UrlDecode(objectKey);

        _logger.LogInformation("[MinIO Webhook] Processing notification for Bucket: '{Bucket}', ObjectKey: '{ObjectKey}'", bucket, decodedObjectKey);

        await _doorbellService.ProcessMinioUploadWebhookAsync(decodedObjectKey, bucket, cancellationToken);
        return Ok(new
        {
            message = "MinIO upload webhook processed successfully.",
            bucket,
            objectKey = decodedObjectKey
        });
    }
}
