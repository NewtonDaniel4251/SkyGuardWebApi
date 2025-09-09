using SkyGuard.Core.Enums;

namespace SkyGuard.Core.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public bool IsAzureAdUser { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpires { get; set; }

        // Navigation properties
        public ICollection<Incident> ReportedIncidents { get; set; }
        public ICollection<Incident> AssignedIncidents { get; set; }
        public ICollection<SecurityResponse> Responses { get; set; }
    }
}
