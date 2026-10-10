using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using MQTTnet;
using MQTTnet.Protocol;

Console.WriteLine("=================================================");
Console.WriteLine("    SMART DOORBELL - CHIME SIMULATOR (MQTTnet)   ");
Console.WriteLine("=================================================");

Console.Write("Enter Indoor Chime Device ID (Guid): ");
var inputChimeId = Console.ReadLine();
if (!Guid.TryParse(inputChimeId, out var chimeId))
{
    chimeId = Guid.Parse("66666666-6666-6666-6666-666666666666"); // Seeded Test Chime Device ID
    Console.WriteLine($"Using test ChimeDeviceId: {chimeId}");
}

var factory = new MqttClientFactory();
using var mqttClient = factory.CreateMqttClient();
using var httpClient = new HttpClient();

var playTopic = $"doorbell/chime/{chimeId}/play";
var ackTopic = $"doorbell/chime/{chimeId}/ack";

mqttClient.ApplicationMessageReceivedAsync += async args =>
{
    var topic = args.ApplicationMessage.Topic;
    var payloadStr = args.ApplicationMessage.ConvertPayloadToString();

    Console.WriteLine($"\n[1. MQTT Command Received] Topic: '{topic}'");
    Console.WriteLine($"[Payload]: {payloadStr}");

    try
    {
        var node = JsonNode.Parse(payloadStr);
        var ringEventIdStr = node?["ringEventId"]?.ToString() ?? node?["RingEventId"]?.ToString() ?? Guid.NewGuid().ToString();
        var toneFileUrl = node?["toneFileUrl"]?.ToString() ?? node?["ToneFileUrl"]?.ToString();
        var ringEventId = Guid.Parse(ringEventIdStr);

        bool playedCloudMelody = false;

        if (!string.IsNullOrWhiteSpace(toneFileUrl))
        {
            try
            {
                Console.WriteLine($"[2. Streaming Cloud Main Melody] Reading audio stream directly from MinIO cloud: '{toneFileUrl}'...");
                using var audioStream = await httpClient.GetStreamAsync(toneFileUrl);
                using var memoryStream = new MemoryStream();
                await audioStream.CopyToAsync(memoryStream);

                Console.WriteLine($"[Cloud Stream Success] Successfully read {memoryStream.Length} bytes of main melody from cloud without saving to disk!");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[🔊 Speaker Audio Playback] Playing Main Melody in Terminal from Cloud: '{toneFileUrl}'...");
                Console.ResetColor();

                // Trigger live system audio playback through speakers
                PlayAudioFromUrl(toneFileUrl);
                playedCloudMelody = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Cloud Stream Error] Could not stream melody from '{toneFileUrl}': {ex.Message}");
            }
        }

        if (!playedCloudMelody)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[🔊 Fallback Audio Playback] No main melody configured for home (or cloud stream unavailable). Playing fallback chime sound in terminal...");
            Console.ResetColor();

            PlayFallbackChimeSound();
        }

        // Send real MQTT ACK to Mosquitto broker
        var ackPayload = JsonSerializer.Serialize(new
        {
            chimeDeviceId = chimeId,
            ringEventId = ringEventId,
            timestamp = DateTime.UtcNow
        });

        Console.WriteLine($"[3. Publishing Real MQTT ACK] Topic: '{ackTopic}'");

        var ackMessage = new MqttApplicationMessageBuilder()
            .WithTopic(ackTopic)
            .WithPayload(ackPayload)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce) // QoS 2
            .Build();

        await mqttClient.PublishAsync(ackMessage);
        Console.WriteLine($"[MQTT QoS 2 ACK Delivered] Sent Chime ACK for RingEvent '{ringEventId}' to Mosquitto broker!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Error processing Chime command] {ex.Message}");
    }
};

var options = new MqttClientOptionsBuilder()
    .WithTcpServer("localhost", 1883)
    .WithClientId($"chime_simulator_{chimeId}")
    .WithCleanSession()
    .Build();

try
{
    Console.WriteLine("[MQTT] Connecting to Mosquitto broker on localhost:1883...");
    await mqttClient.ConnectAsync(options);
    Console.WriteLine("[MQTT] Connected successfully!");

    await mqttClient.SubscribeAsync(new MqttClientSubscribeOptionsBuilder()
        .WithTopicFilter(playTopic)
        .Build());

    Console.WriteLine($"[Subscribed to Topic]: '{playTopic}'");
    Console.WriteLine($"[Publishing ACKs on]: '{ackTopic}'");
}
catch (Exception ex)
{
    Console.WriteLine($"[MQTT Error] Could not connect to Mosquitto: {ex.Message}");
}

Console.WriteLine("\nWaiting for real MQTT Chime Play commands from backend... (Press 'q' + ENTER to exit)");
while (Console.ReadLine()?.ToLower() != "q") { }

// Helper method to play cloud audio URL directly through system speakers using CLI audio engine
static void PlayAudioFromUrl(string url)
{
    try
    {
        // Try ffplay first (FFmpeg silent audio player)
        var psi = new ProcessStartInfo
        {
            FileName = "ffplay",
            Arguments = $"-nodisp -autoexit -loglevel quiet \"{url}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var proc = Process.Start(psi);
        proc?.WaitForExit(6000); // Wait max 6 seconds for chime play
    }
    catch
    {
        try
        {
            // Fallback to mpv
            var psiMpv = new ProcessStartInfo
            {
                FileName = "mpv",
                Arguments = $"--no-video --really-quiet \"{url}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var procMpv = Process.Start(psiMpv);
            procMpv?.WaitForExit(6000);
        }
        catch
        {
            Console.Beep();
        }
    }
}

// Helper method to play system chime bell sound in terminal
static void PlayFallbackChimeSound()
{
    try
    {
        var psi = new ProcessStartInfo
        {
            FileName = "canberra-gtk-play",
            Arguments = "-i phone-incoming-call",
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var proc = Process.Start(psi);
        if (proc != null && !proc.WaitForExit(3000))
        {
            proc.Kill();
        }
    }
    catch
    {
        Console.Beep();
        Console.Write("\a"); // Terminal bell escape character
    }
}
