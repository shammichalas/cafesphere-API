namespace CafeSphere.Infrastructure.Identity;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserEmail { get; }
    string? UserRole { get; }
    string? IpAddress { get; }
    bool IsAuthenticated { get; }
}
