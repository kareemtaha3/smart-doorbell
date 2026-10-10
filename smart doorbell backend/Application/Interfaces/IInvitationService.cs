using smart_doorbell.Application.DTOs;

namespace smart_doorbell.Application.Interfaces;

public interface IInvitationService
{
    Task<InvitationResponseDto> SendInvitationAsync(Guid homeId, Guid invitedByUserId, string invitedUsername);
    Task<IEnumerable<InvitationResponseDto>> GetUserInvitationsAsync(Guid userId);
    Task<InvitationResponseDto> RespondToInvitationAsync(Guid invitationId, Guid userId, bool accept);
}
