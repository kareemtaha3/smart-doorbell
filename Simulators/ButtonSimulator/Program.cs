using System.Text;
using System.Text.Json;
using MQTTnet;
using MQTTnet.Protocol;

Console.WriteLine("=================================================");
Console.WriteLine("    SMART DOORBELL - BUTTON SIMULATOR (MQTTnet)  ");
Console.WriteLine("=================================================");

var factory = new MqttClientFactory();
using var mqttClient = factory.CreateMqttClient();

var options = new MqttClientOptionsBuilder()
    .WithTcpServer("localhost", 1883)
    .WithClientId($"button_simulator_{Guid.NewGuid()}")
    .WithCleanSession()
    .Build();

try
{
    Console.WriteLine("[MQTT] Connecting to Mosquitto broker on localhost:1883...");
    await mqttClient.ConnectAsync(options);
    Console.WriteLine("[MQTT] Connected successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"[MQTT Error] Could not connect to Mosquitto: {ex.Message}");
    Console.WriteLine("Ensure Mosquitto MQTT broker is running on localhost:1883.");
}

Console.Write("\nEnter Button Device ID (Guid): ");
var inputDeviceId = Console.ReadLine();
if (!Guid.TryParse(inputDeviceId, out var buttonDeviceId))
{
    buttonDeviceId = Guid.Parse("44444444-4444-4444-4444-444444444444"); // Seeded Test Button Device ID
    Console.WriteLine($"Using default test ButtonDeviceId: {buttonDeviceId}");
}

var topic = $"doorbell/button/{buttonDeviceId}/ring";
Console.WriteLine($"\n[Target Topic]: {topic}");
Console.WriteLine("Press ENTER to simulate pressing the doorbell button (or type 'q' to quit)...");

while (true)
{
    var input = Console.ReadLine();
    if (input?.ToLower() == "q") break;

    var eventId = $"evt_{DateTime.UtcNow.Ticks}";
    var payloadJson = JsonSerializer.Serialize(new
    {
        buttonDeviceId = buttonDeviceId,
        deviceEventId = eventId,
        timestamp = DateTime.UtcNow
    });

    Console.WriteLine($"\n[Publishing Ring Event] Topic: '{topic}'");
    Console.WriteLine($"[Payload]: {payloadJson}");

    if (mqttClient.IsConnected)
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payloadJson)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce) // QoS 2
            .Build();

        await mqttClient.PublishAsync(message);
        Console.WriteLine("[MQTT QoS 2 Delivered] Ring Event message published to Mosquitto broker!");
    }
    else
    {
        Console.WriteLine("[MQTT Warning] Client not connected to Mosquitto.");
    }

    Console.WriteLine("\nPress ENTER to ring again...");
}
