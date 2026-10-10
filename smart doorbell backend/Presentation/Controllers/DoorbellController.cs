using Microsoft.AspNetCore.Mvc;
using smart_doorbell.Application.DTOs;
using smart_doorbell.Application.Interfaces.Services;

namespace smart_doorbell.Presentation.Controllers;

public record RingRequest(
    Guid ButtonDeviceId,
    string DeviceEventId
);

[ApiController]
[Route("api/doorbell")]
public class DoorbellController : ControllerBase
{
    private readonly IDoorbellService _doorbellService;

    public DoorbellController(IDoorbellService doorbellService)
    {
        _doorbellService = doorbellService;
    }

    [HttpPost("ring")]
    public async Task<ActionResult<RingSessionDetailDto>> IngestRingEvent(
        [FromBody] RingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _doorbellService.ProcessRingEventAsync(request.ButtonDeviceId, request.DeviceEventId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("sessions/{sessionId:guid}/close")]
    public async Task<IActionResult> CloseSession(Guid sessionId, CancellationToken cancellationToken)
    {
        await _doorbellService.CloseSessionAsync(sessionId, cancellationToken);
        return Ok(new { message = "Session closed successfully." });
    }
}
