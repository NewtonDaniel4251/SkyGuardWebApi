using SkyGuard.Core.Enums;

namespace SkyGuard.Core.Models
{
    public class Incident
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public IncidentPriority Priority { get; set; }
        public IncidentStatus Status { get; set; } = IncidentStatus.Pending;
        public AreaType Area { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string PathLine { get; set; }
        public string? ImageLink { get; set; }
        public string? VideoLink { get; set; }
        public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReportedToSecurityAt { get; set; }

        // Foreign keys
        public Guid ReportedById { get; set; }
        public Guid? AssignedToId { get; set; }

        // Navigation properties
        public User ReportedBy { get; set; }
        public User AssignedTo { get; set; }
        public SecurityResponse Response { get; set; }
    }
}
