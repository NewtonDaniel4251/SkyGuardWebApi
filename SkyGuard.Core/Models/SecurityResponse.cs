using SkyGuard.Core.Enums;

namespace SkyGuard.Core.Models
{
    public class SecurityResponse
    {
        public Guid Id { get; set; }
        public string ActionTaken { get; set; }
        public string AdditionalComments { get; set; }
        public IncidentClassification Classification { get; set; }
        public string InterventionImagePath { get; set; }
        public DateTime RespondedAt { get; set; } = DateTime.UtcNow;

        // Foreign key
        public Guid IncidentId { get; set; }
        public Guid RespondedById { get; set; }

        // Navigation properties
        public Incident Incident { get; set; }
        public User RespondedBy { get; set; }
    }
}
