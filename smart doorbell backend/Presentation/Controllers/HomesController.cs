using Microsoft.AspNetCore.Mvc;
using smart_doorbell.Application.DTOs;
using smart_doorbell.Application.Interfaces.Services;

namespace smart_doorbell.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HomesController : ControllerBase
{
    private readonly IHomeService _homeService;

    public HomesController(IHomeService homeService)
    {
        _homeService = homeService;
    }

    [HttpPost]
    public async Task<ActionResult<HomeResponseDto>> CreateHome([FromBody] CreateHomeDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var home = await _homeService.CreateHomeAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = home.Id }, home);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<HomeResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var home = await _homeService.GetHomeByIdAsync(id, cancellationToken);
        if (home == null)
        {
            return NotFound(new { message = $"Home with ID '{id}' not found." });
        }
        return Ok(home);
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<List<HomeResponseDto>>> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var homes = await _homeService.GetHomesByUserIdAsync(userId, cancellationToken);
        return Ok(homes);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<HomeResponseDto>> Update(Guid id, [FromBody] UpdateHomeDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var updatedHome = await _homeService.UpdateHomeAsync(id, dto, cancellationToken);
            return Ok(updatedHome);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _homeService.DeleteHomeAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/members")]
    public async Task<ActionResult<List<HomeMemberResponseDto>>> GetMembers(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var members = await _homeService.GetHomeMembersAsync(id, cancellationToken);
            return Ok(members);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            await _homeService.RemoveHomeMemberAsync(id, userId, cancellationToken);
            return NoContent();
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
}
