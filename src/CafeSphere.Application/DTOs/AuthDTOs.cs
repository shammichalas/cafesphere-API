namespace CafeSphere.Application.DTOs;

public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string FullName,
    string? PhoneNumber
);

public record LoginRequest(
    string EmailOrUsername,
    string Password
);

public record AuthResponse(
    string UserId,
    string Username,
    string Email,
    string FullName,
    string Role,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt
);

public record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken
);

public record ForgotPasswordRequest(
    string Email
);

public record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword
);

public record UserDto(
    string Id,
    string Username,
    string Email,
    string FullName,
    string? PhoneNumber,
    string Role,
    bool IsEmailVerified,
    DateTime CreatedAt
);
