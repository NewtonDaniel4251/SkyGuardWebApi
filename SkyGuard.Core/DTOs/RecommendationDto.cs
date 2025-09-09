namespace SkyGuard.Core.DTOs
{
    public class RecommendationDto
    {
        public string Priority { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Impact { get; set; }
        public DateTime EstimatedCompletion { get; set; }

    }
}
