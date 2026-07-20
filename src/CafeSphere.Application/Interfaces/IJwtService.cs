using System.Security.Claims;
using CafeSphere.Domain.Entities;

namespace CafeSphere.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user, IEnumerable<string>? permissions = null);
    RefreshToken GenerateRefreshToken(string ipAddress);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
