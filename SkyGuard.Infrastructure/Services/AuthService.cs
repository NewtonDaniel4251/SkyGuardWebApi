using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SkyGuard.Core.Enums;
using SkyGuard.Core.Models;
using SkyGuard.Core.Services;
using SkyGuard.Infrastructure.Respositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SkyGuard.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _config;

        public AuthService(IUserRepository userRepository, IConfiguration config)
        {
            _userRepository = userRepository;
            _config = config;
        }

        public async Task<User> Register(string email, string name, string password, UserRole role)
        {
            using var hmac = new HMACSHA512();

            var user = new User
            {
                Email = email.ToLower(),
                Name = name,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password)),
                PasswordSalt = hmac.Key, 
                Role = role,
                RefreshToken = string.Empty,
                RefreshTokenExpires = DateTime.UtcNow.AddDays(7)
            };

            await _userRepository.AddAsync(user);
            return user;
        }

        public async Task<User> Login(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email.ToLower());
            if (user == null) return null;

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return null;
            }

            // Update last login and generate refresh token
            user.LastLogin = DateTime.UtcNow;
            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpires = DateTime.UtcNow.AddDays(7);

            await _userRepository.UpdateAsync(user);
            return user;
        }

        public async Task<bool> UserExists(string email)
        {
            return await _userRepository.GetByEmailAsync(email.ToLower()) != null;
        }

        public string CreateToken(User user)
        {
            if (user.PasswordSalt.Length < 64)
                throw new ArgumentException("Invalid key size");

            var tokenHandler = new JwtSecurityTokenHandler();
            var keyBytes = Encoding.UTF8.GetBytes(GetJwtKey());
            var key = new SymmetricSecurityKey(keyBytes);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(10),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature)
            };

            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }
        public async Task<User> GetUserByRefreshToken(string refreshToken)
        {
            return await _userRepository.GetByRefreshToken(refreshToken);
        }

        public async Task RevokeRefreshToken(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;

            user.RefreshToken = null;
            user.RefreshTokenExpires = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public string GetJwtKey()
        {
            var key = _config["Jwt:SecretKey"];
            if (string.IsNullOrEmpty(key) || key.Length < 64)
            {
                throw new InvalidOperationException("JWT Key must be at least 512 bits (64 characters)");
            }
            return key;
        }
    }
}
