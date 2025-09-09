using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SkyGuard.Core.DTOs;
using SkyGuard.Core.Services;
using SkyGuard.Infrastructure.Services;

namespace SkyGuard.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableRateLimiting("ai-analysis")]
    public class AIAnalysisController : ControllerBase
    {
        private readonly IAIAnalysisService _aiAnalysisService;
        private readonly ILogger<AIAnalysisController> _logger;

        public AIAnalysisController(IAIAnalysisService aiAnalysisService, ILogger<AIAnalysisController> logger)
        {
            _aiAnalysisService = aiAnalysisService;
            _logger = logger;
        }

        [HttpGet("dashboard-insights")]
        [ProducesResponseType(typeof(AIDashboardInsightsDto), 200)]
        [ProducesResponseType(503)]
        public async Task<IActionResult> GetDashboardInsights()
        {
            try
            {
                if (!await _aiAnalysisService.IsAIAvailableAsync())
                {
                    return StatusCode(503, "AI analysis service is currently unavailable");
                }

                var insights = await _aiAnalysisService.GetDashboardInsightsAsync();
                return Ok(insights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard insights");
                return StatusCode(500, "Error generating insights");
            }
        }

        [HttpGet("recommendations")]
        [ProducesResponseType(typeof(List<RecommendationDto>), 200)]
        public async Task<IActionResult> GetRecommendations()
        {
            try
            {
                var recommendations = await _aiAnalysisService.GetRecommendationsAsync();
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recommendations");
                return StatusCode(500, "Error generating recommendations");
            }
        }

        [HttpGet("risk-prediction")]
        [ProducesResponseType(typeof(RiskPredictionDto), 200)]
        public async Task<IActionResult> GetRiskPrediction()
        {
            try
            {
                var riskPrediction = await _aiAnalysisService.GetRiskPredictionAsync();
                return Ok(riskPrediction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving risk prediction");
                return StatusCode(500, "Error generating risk prediction");
            }
        }

        [HttpGet("hotspots")]
        [ProducesResponseType(typeof(HotspotAnalysisDto), 200)]
        public async Task<IActionResult> GetHotspots()
        {
            try
            {
                var hotspots = await _aiAnalysisService.GetHotspotsAsync();
                return Ok(hotspots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hotspots");
                return StatusCode(500, "Error analyzing hotspots");
            }
        }

        [HttpGet("patterns")]
        [ProducesResponseType(typeof(PatternAnalysisDto), 200)]
        public async Task<IActionResult> GetPatterns()
        {
            try
            {
                var patterns = await _aiAnalysisService.GetPatternsAsync();
                return Ok(patterns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patterns");
                return StatusCode(500, "Error analyzing patterns");
            }
        }

        [HttpPost("analyze-incident/{incidentId:guid}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AnalyzeIncident(Guid incidentId, [FromBody] AIAnalysisRequest request)
        {
            try
            {
                var analysis = await _aiAnalysisService.AnalyzeIncidentAsync(incidentId);
                return Ok(analysis);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Incident not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing incident {IncidentId}", incidentId);
                return StatusCode(500, "Error analyzing incident");
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetServiceStatus()
        {
            var isAvailable = await _aiAnalysisService.IsAIAvailableAsync();
            return Ok(new { Available = isAvailable, Timestamp = DateTime.UtcNow });
        }


        [HttpGet("analysis-summary")]
        public async Task<ActionResult<AnalysisSummaryDto>> GetAnalysisSummary()
        {
            try
            {
                var analysis = await _aiAnalysisService.GetAnalysisSummaryAsync();
                return Ok(analysis);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Incident not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving analysis summary");
                return StatusCode(500, "Error analyzing summary");
            }
        }
    }
}
