using smart_doorbell.Application.DTOs;
using smart_doorbell.Application.Interfaces;
using smart_doorbell.Application.Interfaces.Repositories;
using smart_doorbell.Domain.Entities;
using smart_doorbell.Domain.Enums;

namespace smart_doorbell.Application.Services;

public class InvitationService : IInvitationService
{
    private readonly IUnitOfWork _unitOfWork;

    public InvitationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<InvitationResponseDto> SendInvitationAsync(Guid homeId, Guid invitedByUserId, string invitedUsername)
    {
        var home = await _unitOfWork.Homes.GetByIdAsync(homeId);
        if (home == null)
        {
            throw new KeyNotFoundException("Home not found.");
        }

        if (home.OwnerUserId != invitedByUserId)
        {
            throw new InvalidOperationException("Only the home owner can send invitations.");
        }

        var invitedUser = await _unitOfWork.Users.GetByUsernameAsync(invitedUsername);
        if (invitedUser == null)
        {
            throw new KeyNotFoundException($"User with username '{invitedUsername}' not found.");
        }

        if (invitedUser.Id == invitedByUserId)
        {
            throw new InvalidOperationException("You cannot invite yourself to your home.");
        }

        var alreadyMember = await _unitOfWork.HomeMembers.IsMemberAsync(homeId, invitedUser.Id);
        if (alreadyMember)
        {
            throw new InvalidOperationException($"User '{invitedUsername}' is already a member of this home.");
        }

        var existingInvitation = await _unitOfWork.HomeInvitations.GetByHomeAndInvitedUserAsync(homeId, invitedUser.Id);

        if (existingInvitation != null)
        {
            if (existingInvitation.Status == InvitationStatus.Pending)
            {
                throw new InvalidOperationException($"An invitation is already pending for '{invitedUsername}'.");
            }
            
            existingInvitation.Status = InvitationStatus.Pending;
            existingInvitation.InvitedByUserId = invitedByUserId;
            existingInvitation.CreatedAt = DateTime.UtcNow;
            existingInvitation.RespondedAt = null;
        }
        else
        {
            existingInvitation = new HomeInvitation
            {
                Id = Guid.NewGuid(),
                HomeId = homeId,
                InvitedByUserId = invitedByUserId,
                InvitedUserId = invitedUser.Id,
                Status = InvitationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.HomeInvitations.AddAsync(existingInvitation);
        }

        await _unitOfWork.SaveChangesAsync();

        var inviter = await _unitOfWork.Users.GetByIdAsync(invitedByUserId);

        return new InvitationResponseDto(
            existingInvitation.Id,
            home.Id,
            home.Name,
            inviter!.Id,
            inviter.Username,
            invitedUser.Id,
            invitedUser.Username,
            existingInvitation.Status,
            existingInvitation.CreatedAt,
            existingInvitation.RespondedAt
        );
    }

    public async Task<IEnumerable<InvitationResponseDto>> GetUserInvitationsAsync(Guid userId)
    {
        var invitations = await _unitOfWork.HomeInvitations.GetByInvitedUserIdAsync(userId);

        return invitations.Select(i => new InvitationResponseDto(
            i.Id,
            i.HomeId,
            i.Home.Name,
            i.InvitedByUserId,
            i.InvitedByUser.Username,
            i.InvitedUserId,
            i.InvitedUser.Username,
            i.Status,
            i.CreatedAt,
            i.RespondedAt
        ));
    }

    public async Task<InvitationResponseDto> RespondToInvitationAsync(Guid invitationId, Guid userId, bool accept)
    {
        var invitation = await _unitOfWork.HomeInvitations.GetByIdAsync(invitationId);

        if (invitation == null || invitation.InvitedUserId != userId)
        {
            throw new KeyNotFoundException("Invitation not found or not assigned to user.");
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            throw new InvalidOperationException($"Invitation has already been {invitation.Status.ToString().ToLower()}.");
        }

        invitation.Status = accept ? InvitationStatus.Accepted : InvitationStatus.Declined;
        invitation.RespondedAt = DateTime.UtcNow;

        if (accept)
        {
            var alreadyMember = await _unitOfWork.HomeMembers.IsMemberAsync(invitation.HomeId, userId);
            if (!alreadyMember)
            {
                var newMember = new HomeMember
                {
                    Id = Guid.NewGuid(),
                    HomeId = invitation.HomeId,
                    UserId = userId,
                    Role = HomeRole.Member,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.HomeMembers.AddAsync(newMember);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return new InvitationResponseDto(
            invitation.Id,
            invitation.HomeId,
            invitation.Home.Name,
            invitation.InvitedByUserId,
            invitation.InvitedByUser.Username,
            invitation.InvitedUserId,
            invitation.InvitedUser.Username,
            invitation.Status,
            invitation.CreatedAt,
            invitation.RespondedAt
        );
    }
}
