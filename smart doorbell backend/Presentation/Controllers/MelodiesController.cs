using Microsoft.AspNetCore.Mvc;
using smart_doorbell.Application.DTOs;
using smart_doorbell.Application.Interfaces.Services;

namespace smart_doorbell.Presentation.Controllers;

[ApiController]
[Route("api/homes/{homeId:guid}/melodies")]
public class MelodiesController : ControllerBase
{
    private readonly IMelodyService _melodyService;

    public MelodiesController(IMelodyService melodyService)
    {
        _melodyService = melodyService;
    }

    [HttpPost]
    public async Task<ActionResult<MelodyResponseDto>> UploadMelody(
        Guid homeId,
        [FromQuery] Guid userId,
        [FromForm] string name,
        [FromForm] bool isMain,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Audio file is required." });
        }

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _melodyService.UploadMelodyAsync(homeId, userId, name, stream, file.FileName, isMain, cancellationToken);
            return CreatedAtAction(nameof(GetHomeMelodies), new { homeId }, result);
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

    [HttpPut("{melodyId:guid}/set-main")]
    public async Task<ActionResult<MelodyResponseDto>> SetMainMelody(
        Guid homeId,
        Guid melodyId,
        [FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _melodyService.SetMainMelodyAsync(homeId, melodyId, userId, cancellationToken);
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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MelodyResponseDto>>> GetHomeMelodies(Guid homeId, CancellationToken cancellationToken)
    {
        var result = await _melodyService.GetHomeMelodiesAsync(homeId, cancellationToken);
        return Ok(result);
    }
}
