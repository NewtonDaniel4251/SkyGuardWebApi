using Microsoft.EntityFrameworkCore;
using SkyGuard.Core.Enums;
using SkyGuard.Core.Models;
using SkyGuard.Infrastructure.Data;

namespace SkyGuard.Infrastructure.Respositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context) { }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
        {
            return await _context.Users.Where(u => u.Role == role).ToListAsync();
        }
        public async Task<User> GetByRefreshToken(string refreshToken)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken &&
                                        u.RefreshTokenExpires > DateTime.UtcNow) ?? new User();
        }
    }
}
