using Microsoft.AspNetCore.Mvc;
using smart_doorbell.Application.DTOs;
using smart_doorbell.Application.Interfaces.Services;

namespace smart_doorbell.Presentation.Controllers;

[ApiController]
[Route("api/camera")]
public class CameraBrokerController : ControllerBase
{
    private readonly ICameraBrokerService _cameraBrokerService;

    public CameraBrokerController(ICameraBrokerService cameraBrokerService)
    {
        _cameraBrokerService = cameraBrokerService;
    }

    [HttpPost("upload-credentials")]
    public async Task<ActionResult<CameraUploadCredentialsResponse>> GetUploadCredentials(
        [FromBody] CameraUploadCredentialsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var credentials = await _cameraBrokerService.GetUploadCredentialsAsync(
                request.CameraDeviceId,
                request.AuthToken,
                request.ObjectKey,
                cancellationToken);

            return Ok(credentials);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}
