namespace smart_doorbell.Application.DTOs;

public record RegisterDto(
    string Username,
    string Email,
    string Password,
    string DisplayName
);

public record LoginDto(
    string UsernameOrEmail,
    string Password
);

public record UserResponseDto(
    Guid Id,
    string Username,
    string Email,
    string DisplayName,
    DateTime CreatedAt
);
