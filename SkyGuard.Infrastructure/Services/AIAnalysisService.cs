using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SkyGuard.Core.DTOs;
using SkyGuard.Core.Models;
using SkyGuard.Core.Services;
using SkyGuard.Infrastructure.Respositories;

namespace SkyGuard.Infrastructure.Services
{
    public class AIAnalysisService : IAIAnalysisService
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly ISecurityResponseRepository _responseRepository;
        private readonly IOpenAIService _openAIService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AIAnalysisService> _logger;

        public AIAnalysisService(
            IIncidentRepository incidentRepository,
            ISecurityResponseRepository responseRepository,
            IOpenAIService openAIService,
            IMemoryCache cache,
            ILogger<AIAnalysisService> logger)
        {
            _incidentRepository = incidentRepository;
            _responseRepository = responseRepository;
            _openAIService = openAIService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<AIDashboardInsightsDto> GetDashboardInsightsAsync()
        {
            var cacheKey = "dashboard_insights";

            if (_cache.TryGetValue(cacheKey, out AIDashboardInsightsDto cachedInsights))
            {
                return cachedInsights;
            }

            try
            {
                var incidents = await _incidentRepository.GetRecentIncidentsAsync(100);
                var responses = await _responseRepository.GetRecentResponsesAsync(100);

                var systemPrompt = @"Return comprehensive dashboard insights in this exact JSON format:
            {
                ""hottestLocation"": {
                    ""area"": ""string"",
                    ""incidentCount"": 0,
                    ""riskLevel"": ""string"",
                    ""trend"": ""string"",
                    ""coordinates"": [{""latitude"": 0.0, ""longitude"": 0.0}]
                },
                ""riskPrediction"": {
                    ""level"": ""string"",
                    ""confidence"": 0,
                    ""factors"": [""string""],
                    ""predictedUntil"": ""2024-01-01T00:00:00Z""
                },
                ""recommendations"": [
                    {
                        ""priority"": ""string"",
                        ""category"": ""string"",
                        ""description"": ""string"",
                        ""impact"": ""string"",
                        ""estimatedCompletion"": ""2024-01-01T00:00:00Z""
                    }
                ],
                ""patterns"": {
                    ""peakHours"": [0],
                    ""seasonalTrend"": ""string"",
                    ""weeklyPattern"": ""string"",
                    ""anomalies"": [""string""]
                },
                ""generatedAt"": ""2024-01-01T00:00:00Z"",
                ""dataPoints"": 0,
                ""confidence"": 0.0
            }";

                var insights = await _openAIService.AnalyzeWithStructuredResponseAsync<AIDashboardInsightsDto>(
                    incidents, responses, systemPrompt);

                insights.GeneratedAt = DateTime.UtcNow;
                insights.DataPoints = incidents.Count + responses.Count;

                _cache.Set(cacheKey, insights, TimeSpan.FromMinutes(30));
                return insights;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating dashboard insights");
                return GetFallbackDashboardInsights();
            }
        }

        public async Task<List<RecommendationDto>> GetRecommendationsAsync()
        {
            // Similar implementation for recommendations
            var incidents = await _incidentRepository.GetRecentIncidentsAsync(50);
            var responses = await _responseRepository.GetRecentResponsesAsync(50);

            var systemPrompt = @"Return actionable recommendations in JSON format...{ 
            ""recommendations"": [
                    {
                        ""priority"": ""string"",
                        ""category"": ""string"",
                        ""description"": ""string"",
                        ""impact"": ""string"",
                        ""estimatedCompletion"": ""2024-01-01T00:00:00Z""
                    }
                ]}";

            return await _openAIService.AnalyzeWithStructuredResponseAsync<List<RecommendationDto>>(
                incidents, responses, systemPrompt);
        }


        public async Task<AnalysisSummaryDto> GetAnalysisSummaryAsync()
        {
            var incidents = await _incidentRepository.GetRecentIncidentsAsync(50);
            var responses = await _responseRepository.GetRecentResponsesAsync(50);

            var systemPrompt = @"You are a security analysis expert. 
            Return your analysis in this exact JSON format:
            {
              ""analysisSummary"": {
                ""totalIncidentsReported"": 0,
                ""totalResponsesReceived"": 0,
                ""incidentsWithCompletedStatus"": 0,
                ""incidentsInProgress"": 0,
                ""criticalPriorityIncidents"": 0,
                ""highPriorityIncidents"": 0,
                ""mediumPriorityIncidents"": 0
              },
              ""insights"": {
                ""criticalIncidents"": [
                  { ""type"": ""string"", ""location"": ""string"", ""status"": ""string"" }
                ],
                ""highPriorityIncidents"": [
                  { ""type"": ""string"", ""location"": ""string"", ""status"": ""string"" }
                ],
                ""mediumPriorityIncidents"": [
                  { ""type"": ""string"", ""location"": ""string"", ""status"": ""string"" }
                ],
                ""responseAnalysis"": {
                  ""ActiveIRPointResponse"": {
                    ""description"": ""string"",
                    ""effectiveness"": ""string"",
                    ""recommendation"": ""string""
                  }
                  // ...other response types
                }
              },
              ""recommendations"": {
                ""immediateActions"": [
                  { ""action"": ""string"", ""details"": ""string"" }
                ],
                ""longTermStrategies"": [
                  { ""action"": ""string"", ""details"": ""string"" }
                ]
              }
            }
            Use the provided incident and response data to populate the fields.";


            return await _openAIService.AnalyzeWithStructuredResponseAsync<AnalysisSummaryDto>(
                incidents.ToList(), responses.ToList(), systemPrompt);
        }

        public async Task<RiskPredictionDto> GetRiskPredictionAsync()
        {
            // Implementation for risk prediction
            var incidents = await _incidentRepository.GetLastMonthsIncidentsAsync(6);

            var systemPrompt = @"Return risk prediction in JSON format...";

            return await _openAIService.AnalyzeWithStructuredResponseAsync<RiskPredictionDto>(
                incidents, new List<SecurityResponse>(), systemPrompt);
        }

        public async Task<HotspotAnalysisDto> GetHotspotsAsync()
        {
            var incidents = await _incidentRepository.GetAllIncidentsAsync();

            var systemPrompt = @"Return hotspot analysis in JSON format...";

            return await _openAIService.AnalyzeWithStructuredResponseAsync<HotspotAnalysisDto>(
                incidents, new List<SecurityResponse>(), systemPrompt);
        }

        public async Task<PatternAnalysisDto> GetPatternsAsync()
        {
            var incidents = await _incidentRepository.GetLastYearsIncidentsAsync();

            var systemPrompt = @"Return pattern analysis in JSON format...";

            return await _openAIService.AnalyzeWithStructuredResponseAsync<PatternAnalysisDto>(
                incidents, new List<SecurityResponse>(), systemPrompt);
        }

        public async Task<object> AnalyzeIncidentAsync(Guid incidentId)
        {
            var incident = await _incidentRepository.GetByIdAsync(incidentId);
            var responses = await _responseRepository.GetByIncidentIdAsync(incidentId);

            var analysis = await _openAIService.AnalyzeIncidentsAsync(
                new List<Incident> { incident }, new List<SecurityResponse> { responses }, "incident");

            return new { Analysis = analysis, Incident = incident };
        }

        public async Task<bool> IsAIAvailableAsync()
        {
            return await _openAIService.ValidateConfiguration();
        }

        private AIDashboardInsightsDto GetFallbackDashboardInsights()
        {
            return new AIDashboardInsightsDto
            {
                HottestLocation = new HotspotAnalysisDto
                {
                    Area = "LAR",
                    IncidentCount = 0,
                    RiskLevel = "Medium",
                    Trend = "Stable"
                },
                RiskPrediction = new RiskPredictionDto
                {
                    Level = "Medium",
                    Confidence = 50,
                    Factors = new List<string> { "Insufficient data" }
                },
                Recommendations = new List<RecommendationDto>(),
                Patterns = new PatternAnalysisDto
                {
                    PeakHours = new List<int>(),
                    SeasonalTrend = "Unknown",
                    WeeklyPattern = "Unknown"
                },
                GeneratedAt = DateTime.UtcNow,
                DataPoints = 0,
                Confidence = 0.5
            };
        }
    }
}
