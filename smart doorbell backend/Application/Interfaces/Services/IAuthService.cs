using smart_doorbell.Application.DTOs;

namespace smart_doorbell.Application.Interfaces.Services;

public interface IAuthService
{
    Task<UserResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);
    Task<UserResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<UserResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
