using SkyGuard.Core.Enums;
using SkyGuard.Core.Models;

namespace SkyGuard.Core.Services
{
    public interface IAuthService
    {
        Task<User> Register(string email, string name, string password, UserRole role);
        Task<User> Login(string email, string password);
        Task<bool> UserExists(string email);
        string CreateToken(User user);
        Task<User> GetUserByRefreshToken(string refreshToken);
        Task RevokeRefreshToken(Guid userId);
        string GetJwtKey();

    }
}
