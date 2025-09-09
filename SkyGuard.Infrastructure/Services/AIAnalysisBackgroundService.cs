using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SkyGuard.Core.Services;

namespace SkyGuard.Infrastructure.Services
{
    public class AIAnalysisBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AIAnalysisBackgroundService> _logger;
        private readonly TimeSpan _analysisInterval = TimeSpan.FromHours(1);

        public AIAnalysisBackgroundService(IServiceProvider serviceProvider, ILogger<AIAnalysisBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AI Analysis Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var analysisService = scope.ServiceProvider.GetRequiredService<IAIAnalysisService>();

                    if (await analysisService.IsAIAvailableAsync())
                    {
                        _logger.LogInformation("Starting periodic AI analysis");

                        //// Pre-cache dashboard insights
                        await analysisService.GetDashboardInsightsAsync();

                        //// Pre-cache recommendations
                        await analysisService.GetRecommendationsAsync();

                        //// Pre-cache anaylsis summary
                        await analysisService.GetAnalysisSummaryAsync();

                        _logger.LogInformation("Periodic AI analysis completed");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during periodic AI analysis");
                }

                await Task.Delay(_analysisInterval, stoppingToken);
            }
        }
    }
}
