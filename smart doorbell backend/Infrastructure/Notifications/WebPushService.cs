using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebPush;

namespace smart_doorbell.Infrastructure.Notifications;

public interface IWebPushService
{
    Task SendPushNotificationAsync(string endpoint, string p256dhKey, string authKey, object payload);
}

public class WebPushService : IWebPushService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebPushService> _logger;

    public WebPushService(IConfiguration configuration, ILogger<WebPushService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendPushNotificationAsync(string endpoint, string p256dhKey, string authKey, object payload)
    {
        var subject = _configuration["Vapid:Subject"] ?? "mailto:admin@smartdoorbell.local";
        var publicKey = _configuration["Vapid:PublicKey"];
        var privateKey = _configuration["Vapid:PrivateKey"];

        if (string.IsNullOrEmpty(publicKey) || string.IsNullOrEmpty(privateKey))
        {
            _logger.LogWarning("VAPID keys not configured. WebPush payload dispatch skipped for endpoint {Endpoint}", endpoint);
            return;
        }

        var subscription = new PushSubscription(endpoint, p256dhKey, authKey);
        var vapidDetails = new VapidDetails(subject, publicKey, privateKey);
        var client = new WebPushClient();

        var jsonPayload = JsonSerializer.Serialize(payload);

        try
        {
            await client.SendNotificationAsync(subscription, jsonPayload, vapidDetails);
        }
        catch (WebPushException ex)
        {
            _logger.LogError(ex, "Failed to send WebPush notification to {Endpoint}. Status: {StatusCode}", endpoint, ex.StatusCode);
            throw;
        }
    }
}
