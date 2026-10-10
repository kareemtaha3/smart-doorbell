using Microsoft.AspNetCore.Mvc;
using smart_doorbell.Application.Interfaces;

namespace smart_doorbell.Presentation.Controllers;

public record ChimeAckRequest(Guid ChimeDeviceId, Guid RingEventId);
public record CameraAckRequest(Guid CameraDeviceId, Guid SessionId);

[ApiController]
[Route("api/mqtt/ack")]
public class MqttAckController : ControllerBase
{
    private readonly IMqttClientService _mqttService;

    public MqttAckController(IMqttClientService mqttService)
    {
        _mqttService = mqttService;
    }

    [HttpPost("chime")]
    public async Task<IActionResult> ProcessChimeAck([FromBody] ChimeAckRequest request, CancellationToken cancellationToken)
    {
        await _mqttService.ProcessChimeAckAsync(request.ChimeDeviceId, request.RingEventId, cancellationToken);
        return Ok(new { message = "Chime ACK processed successfully." });
    }

    [HttpPost("camera")]
    public async Task<IActionResult> ProcessCameraAck([FromBody] CameraAckRequest request, CancellationToken cancellationToken)
    {
        await _mqttService.ProcessCameraAckAsync(request.CameraDeviceId, request.SessionId, cancellationToken);
        return Ok(new { message = "Camera ACK processed successfully." });
    }
}
