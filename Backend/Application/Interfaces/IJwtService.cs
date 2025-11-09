using System.Security.Claims;
using Backend.Domain.Entities.Identity;

namespace Backend.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles, IList<string> permissions);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    Task<RefreshToken> CreateRefreshTokenAsync(ApplicationUser user, string token, string ipAddress);
    Task<bool> RevokeRefreshTokenAsync(string token, string ipAddress, string reason);
}
