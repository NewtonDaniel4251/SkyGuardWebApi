namespace SkyGuard.Core.Models
{
    public class AuditLogEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Foreign key to User
        public Guid UserId { get; set; }
        public User User { get; set; } 

        public string Action { get; set; }
        public string Resource { get; set; }
        public string ResourceId { get; set; }
        public string Details { get; set; }

        public string IpAddress { get; set; }
        public string UserAgent { get; set; }

        public bool Success { get; set; }
    }
}