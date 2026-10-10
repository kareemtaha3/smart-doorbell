using Microsoft.AspNetCore.Mvc;
using smart_doorbell.Application.DTOs;
using smart_doorbell.Application.Interfaces.Services;

namespace smart_doorbell.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserResponseDto>> Register([FromBody] RegisterDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _authService.RegisterAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserResponseDto>> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _authService.LoginAsync(dto, cancellationToken);
            return Ok(user);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _authService.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }
}
