using SkyGuard.Core.Enums;
using SkyGuard.Core.Models;

namespace SkyGuard.Infrastructure.Respositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetByRoleAsync(UserRole role);
        Task<User> GetByRefreshToken(string refreshToken);
    }
}
