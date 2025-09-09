namespace SkyGuard.Core.DTOs
{
    public class IncidentDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string Area { get; set; }
        public DateTime ReportedAt { get; set; }
        public DateTime ReportedToSecurityAt { get; set; }
        public string ReportedBy { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string PipelineLocation { get; set; }

        public string? ImageSharePointUrl { get; set; }

        public string? VideoSharePointUrl { get; set; }
        public string Description { get; set; }
    }
}
