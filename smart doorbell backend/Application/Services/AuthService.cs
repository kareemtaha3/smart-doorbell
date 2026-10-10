using smart_doorbell.Application.DTOs;
using smart_doorbell.Application.Interfaces.Repositories;
using smart_doorbell.Application.Interfaces.Services;
using smart_doorbell.Domain.Entities;

namespace smart_doorbell.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UserResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        var username = dto.Username.Trim();
        var email = dto.Email.Trim();

        if (await _unitOfWork.Users.ExistsByUsernameAsync(username, cancellationToken))
        {
            throw new InvalidOperationException($"Username '{username}' is already taken.");
        }

        if (await _unitOfWork.Users.ExistsByEmailAsync(email, cancellationToken))
        {
            throw new InvalidOperationException($"Email '{email}' is already registered.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = dto.Password, // For test build
            DisplayName = dto.DisplayName.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UserResponseDto(
            user.Id,
            user.Username,
            user.Email,
            user.DisplayName,
            user.CreatedAt
        );
    }

    public async Task<UserResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var input = dto.UsernameOrEmail.Trim();
        var user = await _unitOfWork.Users.GetByUsernameAsync(input, cancellationToken)
            ?? await _unitOfWork.Users.GetByEmailAsync(input, cancellationToken);

        if (user == null || user.PasswordHash != dto.Password)
        {
            throw new UnauthorizedAccessException("Invalid username/email or password.");
        }

        return new UserResponseDto(
            user.Id,
            user.Username,
            user.Email,
            user.DisplayName,
            user.CreatedAt
        );
    }

    public async Task<UserResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (user == null) return null;

        return new UserResponseDto(
            user.Id,
            user.Username,
            user.Email,
            user.DisplayName,
            user.CreatedAt
        );
    }
}
