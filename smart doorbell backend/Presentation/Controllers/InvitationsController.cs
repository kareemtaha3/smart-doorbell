using Microsoft.AspNetCore.Mvc;
using smart_doorbell.Application.DTOs;
using smart_doorbell.Application.Interfaces;

namespace smart_doorbell.Presentation.Controllers;

[ApiController]
[Route("api")]
public class InvitationsController : ControllerBase
{
    private readonly IInvitationService _invitationService;

    public InvitationsController(IInvitationService invitationService)
    {
        _invitationService = invitationService;
    }

    [HttpPost("homes/{homeId:guid}/invitations")]
    public async Task<ActionResult<InvitationResponseDto>> SendInvitation(
        Guid homeId,
        [FromQuery] Guid invitedByUserId,
        [FromBody] CreateInvitationDto dto)
    {
        try
        {
            var result = await _invitationService.SendInvitationAsync(homeId, invitedByUserId, dto.InvitedUsername);
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

    [HttpGet("invitations")]
    public async Task<ActionResult<IEnumerable<InvitationResponseDto>>> GetUserInvitations([FromQuery] Guid userId)
    {
        var invitations = await _invitationService.GetUserInvitationsAsync(userId);
        return Ok(invitations);
    }

    [HttpPost("invitations/{id:guid}/respond")]
    public async Task<ActionResult<InvitationResponseDto>> RespondToInvitation(
        Guid id,
        [FromQuery] Guid userId,
        [FromBody] RespondInvitationDto dto)
    {
        try
        {
            var result = await _invitationService.RespondToInvitationAsync(id, userId, dto.Accept);
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
}
