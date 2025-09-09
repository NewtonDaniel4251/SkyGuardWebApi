using System.Security.Claims;

namespace SkyGuard.Core.Services
{
    public interface IAzureAdTokenService
    {
        Task<ClaimsPrincipal> ValidateTokenAsync(string token);
    }
}
