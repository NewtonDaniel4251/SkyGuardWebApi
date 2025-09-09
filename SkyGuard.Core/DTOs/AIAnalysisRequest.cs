namespace SkyGuard.Core.DTOs
{
    public class AIAnalysisRequest
    {
        public string IncidentId { get; set; }
        public string AnalysisType { get; set; }
        public Dictionary<string, object> AdditionalContext { get; set; }
    }
}
