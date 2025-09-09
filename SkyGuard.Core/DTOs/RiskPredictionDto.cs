namespace SkyGuard.Core.DTOs
{
    public class RiskPredictionDto
    {
        public string Level { get; set; }
        public int Confidence { get; set; }
        public List<string> Factors { get; set; }
        public DateTime PredictedUntil { get; set; }
    }
}
