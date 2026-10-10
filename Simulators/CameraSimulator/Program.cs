using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Protocol;

Console.WriteLine("=================================================");
Console.WriteLine("    SMART DOORBELL - CAMERA SIMULATOR (MQTTnet)  ");
Console.WriteLine("=================================================");

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .Build();

var snapshotsDirectory = config["CameraOptions:SnapshotsPath"];

if (string.IsNullOrWhiteSpace(snapshotsDirectory) || !Directory.Exists(snapshotsDirectory))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n[CRITICAL WARNING] No valid 'SnapshotsPath' configured in appsettings.json / appsettings.Development.json or folder '{snapshotsDirectory}' does not exist.");
    Console.WriteLine("Camera simulator cannot start. Please configure a valid folder containing snapshot images in appsettings.Development.json.\n");
    Console.ResetColor();
    Environment.Exit(1);
    return;
}

var imageFiles = Directory.GetFiles(snapshotsDirectory, "*.*")
    .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                f.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
    .OrderBy(f => f)
    .ToArray();

if (imageFiles.Length == 0)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n[CRITICAL WARNING] Configured snapshots folder '{snapshotsDirectory}' is EMPTY (no .jpg, .jpeg, or .png images found).");
    Console.WriteLine("Camera simulator cannot start. Please add snapshot images to the directory.\n");
    Console.ResetColor();
    Environment.Exit(1);
    return;
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"\n[Configured Snapshots Path]: '{snapshotsDirectory}'");
Console.WriteLine($"[Loaded Images Count]: {imageFiles.Length} image(s) available for sequential round-robin upload.");
Console.ResetColor();

Console.Write("\nEnter Outdoor Camera Device ID (Guid): ");
var inputCameraId = Console.ReadLine();
if (!Guid.TryParse(inputCameraId, out var cameraId))
{
    cameraId = Guid.Parse("55555555-5555-5555-5555-555555555555"); // Seeded Test Camera Device ID
    Console.WriteLine($"Using test CameraDeviceId: {cameraId}");
}

Console.Write("Enter Camera AuthToken (shared secret): ");
var authToken = Console.ReadLine();
if (string.IsNullOrWhiteSpace(authToken))
{
    authToken = "token-camera-device-456"; // Seeded Camera AuthToken
}

var backendUrl = config["CameraOptions:BackendUrl"] ?? "http://localhost:5147";
Console.WriteLine($"[Backend URL Configured]: {backendUrl}");
var httpClient = new HttpClient { BaseAddress = new Uri(backendUrl) };
var factory = new MqttClientFactory();
using var mqttClient = factory.CreateMqttClient();

var snapshotTopic = $"doorbell/camera/{cameraId}/snapshot";
var ackTopic = $"doorbell/camera/{cameraId}/ack";
int currentImageIndex = 0;

mqttClient.ApplicationMessageReceivedAsync += async args =>
{
    var topic = args.ApplicationMessage.Topic;
    var payloadStr = args.ApplicationMessage.ConvertPayloadToString();

    Console.WriteLine($"\n[1. MQTT Snapshot Command Received] Topic: '{topic}'");
    Console.WriteLine($"[Payload]: {payloadStr}");

    try
    {
        var node = JsonNode.Parse(payloadStr);
        var sessionIdStr = node?["sessionId"]?.ToString() ?? node?["SessionId"]?.ToString() ?? Guid.NewGuid().ToString();
        var objectKeyStr = node?["objectKey"]?.ToString() ?? node?["ObjectKey"]?.ToString() ?? $"snapshots/home/{sessionIdStr}.jpg";
        var sessionId = Guid.Parse(sessionIdStr);

        // 1. Send real MQTT ACK to Mosquitto broker
        var ackPayload = JsonSerializer.Serialize(new
        {
            cameraDeviceId = cameraId,
            sessionId = sessionId,
            timestamp = DateTime.UtcNow
        });

        Console.WriteLine($"[2. Publishing Real MQTT ACK] Topic: '{ackTopic}'");

        var ackMessage = new MqttApplicationMessageBuilder()
            .WithTopic(ackTopic)
            .WithPayload(ackPayload)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce) // QoS 2
            .Build();

        await mqttClient.PublishAsync(ackMessage);
        Console.WriteLine($"[MQTT QoS 2 ACK Delivered] Sent Camera ACK for Session '{sessionId}' to Mosquitto broker!");

        // 2. Request upload credentials from backend broker
        Console.WriteLine("\n[3. Requesting Cloud Upload Credentials from Backend Broker]");
        var credResponse = await httpClient.PostAsJsonAsync("/api/camera/upload-credentials", new
        {
            CameraDeviceId = cameraId,
            AuthToken = authToken,
            ObjectKey = objectKeyStr
        });

        if (credResponse.IsSuccessStatusCode)
        {
            var json = await credResponse.Content.ReadFromJsonAsync<JsonObject>();
            var uploadUrl = json?["uploadUrl"]?.ToString();
            var bucket = json?["bucket"]?.ToString() ?? "doorbell-snapshots";

            Console.WriteLine($"[Credentials Granted] Presigned Upload URL: {uploadUrl}");

            // 3. Round-Robin Image Selection (one by one sequentially, loops back when reaching end)
            var selectedImageIndex = currentImageIndex % imageFiles.Length;
            var selectedImageFile = imageFiles[selectedImageIndex];
            currentImageIndex++;

            Console.WriteLine($"[4. Sequential Round-Robin Image #{currentImageIndex}] Selected Image ({selectedImageIndex + 1}/{imageFiles.Length}): '{Path.GetFileName(selectedImageFile)}' ({new FileInfo(selectedImageFile).Length} bytes)");

            // 4. HTTP PUT upload to MinIO
            using var fileStream = File.OpenRead(selectedImageFile);
            using var streamContent = new StreamContent(fileStream);
            var extension = Path.GetExtension(selectedImageFile).ToLower();
            var contentType = extension == ".png" ? "image/png" : "image/jpeg";
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            Console.WriteLine($"[5. Cloud Upload to MinIO] HTTP PUT image payload to MinIO '{bucket}/{objectKeyStr}'...");
            var uploadResult = await httpClient.PutAsync(uploadUrl, streamContent);

            Console.WriteLine($"[MinIO Upload Response Status] {uploadResult.StatusCode}");

            // 5. Notify backend via MinIO webhook
            // Console.WriteLine("[6. MinIO Upload Webhook] Notifying backend of completed upload...");
            // var webhookResponse = await httpClient.PostAsJsonAsync("/api/webhooks/minio", new
            // {
            //     ObjectKey = objectKeyStr,
            //     Bucket = bucket
            // });

            // Console.WriteLine($"[MinIO Webhook Status] {webhookResponse.StatusCode} - Visitor photo updated on Notification!");
        }
        else
        {
            Console.WriteLine($"[Broker Error] {credResponse.StatusCode} - {await credResponse.Content.ReadAsStringAsync()}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Camera Processing Error] {ex.Message}");
    }
};

var options = new MqttClientOptionsBuilder()
    .WithTcpServer("localhost", 1883)
    .WithClientId($"camera_simulator_{cameraId}")
    .WithCleanSession()
    .Build();

try
{
    Console.WriteLine("[MQTT] Connecting to Mosquitto broker on localhost:1883...");
    await mqttClient.ConnectAsync(options);
    Console.WriteLine("[MQTT] Connected successfully!");

    await mqttClient.SubscribeAsync(new MqttClientSubscribeOptionsBuilder()
        .WithTopicFilter(snapshotTopic)
        .Build());

    Console.WriteLine($"[Subscribed to Topic]: '{snapshotTopic}'");
    Console.WriteLine($"[Publishing ACKs on]: '{ackTopic}'");
}
catch (Exception ex)
{
    Console.WriteLine($"[MQTT Error] Could not connect to Mosquitto: {ex.Message}");
}

Console.WriteLine("\nWaiting for real MQTT Camera Snapshot commands... (Press 'q' + ENTER to exit)");
while (Console.ReadLine()?.ToLower() != "q") { }
