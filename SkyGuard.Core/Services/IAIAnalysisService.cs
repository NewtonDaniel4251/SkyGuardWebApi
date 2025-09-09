using SkyGuard.Core.DTOs;

namespace SkyGuard.Core.Services
{
    public interface IAIAnalysisService
    {
        Task<AIDashboardInsightsDto> GetDashboardInsightsAsync();
        Task<List<RecommendationDto>> GetRecommendationsAsync();
        Task<RiskPredictionDto> GetRiskPredictionAsync();
        Task<HotspotAnalysisDto> GetHotspotsAsync();
        Task<PatternAnalysisDto> GetPatternsAsync();
        Task<AnalysisSummaryDto> GetAnalysisSummaryAsync();
        Task<object> AnalyzeIncidentAsync(Guid incidentId);
        Task<bool> IsAIAvailableAsync();
    }
}
