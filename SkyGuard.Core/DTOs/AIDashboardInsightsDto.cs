namespace SkyGuard.Core.DTOs
{
    public class AIDashboardInsightsDto
    {
        public HotspotAnalysisDto HottestLocation { get; set; }
        public RiskPredictionDto RiskPrediction { get; set; }
        public List<RecommendationDto> Recommendations { get; set; }
        public PatternAnalysisDto Patterns { get; set; }
        public DateTime GeneratedAt { get; set; }
        public int DataPoints { get; set; }
        public double Confidence { get; set; }
    }
}
