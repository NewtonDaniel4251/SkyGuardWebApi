
namespace SkyGuard.Core.DTOs
{
    public class AnalysisSummaryDto
    {
        public AnalysisSummarySection AnalysisSummary { get; set; }
        public InsightsSection Insights { get; set; }
        public RecommendationsSection Recommendations { get; set; }
    }

    public class AnalysisSummarySection
    {
        public int TotalIncidentsReported { get; set; }
        public int TotalResponsesReceived { get; set; }
        public int IncidentsWithCompletedStatus { get; set; }
        public int IncidentsInProgress { get; set; }
        public int CriticalPriorityIncidents { get; set; }
        public int HighPriorityIncidents { get; set; }
        public int MediumPriorityIncidents { get; set; }
    }

    public class InsightsSection
    {
        public List<IncidentInsight> CriticalIncidents { get; set; }
        public List<IncidentInsight> HighPriorityIncidents { get; set; }
        public List<IncidentInsight> MediumPriorityIncidents { get; set; }
        public Dictionary<string, ResponseAnalysisDto> ResponseAnalysis { get; set; }
    }

    public class IncidentInsight
    {
        public string Type { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
    }

    public class ResponseAnalysisDto
    {
        public string Description { get; set; }
        public string Effectiveness { get; set; }
        public string Recommendation { get; set; }
    }

    public class RecommendationsSection
    {
        public List<RecommendationActionDto> ImmediateActions { get; set; }
        public List<RecommendationActionDto> LongTermStrategies { get; set; }
    }

    public class RecommendationActionDto
    {
        public string Action { get; set; }
        public string Details { get; set; }
    }
}
