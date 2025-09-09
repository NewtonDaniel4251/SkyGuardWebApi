using SkyGuard.Core.Models;

namespace SkyGuard.Core.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }

    }

    public class LoginResponse
    {
        public string token { get; set; }
        public UserDto user { get; set; }
        public int expiresIn { get; set; }
    }
}
