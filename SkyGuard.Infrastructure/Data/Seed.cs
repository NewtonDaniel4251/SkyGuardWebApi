using Microsoft.EntityFrameworkCore;
using SkyGuard.Core.Enums;
using SkyGuard.Core.Models;
using System.Security.Cryptography;
using System.Text;

namespace SkyGuard.Infrastructure.Data
{
    public static class Seed
    {
        public static async Task SeedUsers(AppDbContext context)
        {
            if (await context.Users.AnyAsync()) return;

            var users = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Admin User",
                Email = "admin@skyguard.com",
                Role = UserRole.Manager,
                PasswordHash = new byte[64], 
                PasswordSalt = new byte[128]  
            },
            new User
            {
                Id = Guid.NewGuid(),
                Name = "UAS Coordinator",
                Email = "uas@skyguard.com",
                Role = UserRole.UASCoordinator,
                PasswordHash = new byte[64],
                PasswordSalt = new byte[128]
            },
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Security Officer 1",
                Email = "security1@skyguard.com",
                Role = UserRole.SecurityTeam,
                PasswordHash = new byte[64],
                PasswordSalt = new byte[128]
            },
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Security Officer 2",
                Email = "security2@skyguard.com",
                Role = UserRole.SecurityTeam,
                PasswordHash = new byte[64],
                PasswordSalt = new byte[128]
            }
        };

            // Set passwords (in production, this would be done through registration)
            foreach (var user in users)
            {
                using var hmac = new HMACSHA512();
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd"));
                user.PasswordSalt = hmac.Key; 
                user.RefreshToken = string.Empty;
                user.RefreshTokenExpires = DateTime.UtcNow.AddDays(7);
            }

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }
    }
}
