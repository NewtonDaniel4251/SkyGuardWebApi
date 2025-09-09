using SkyGuard.Core.Models;

namespace SkyGuard.Core.Services
{
    public interface IOpenAIService
    {
        Task<string> AnalyzeIncidentsAsync(List<Incident> incidents, List<SecurityResponse> responses, string analysisType);
        Task<T> AnalyzeWithStructuredResponseAsync<T>(List<Incident> incidents, List<SecurityResponse> responses, string systemPrompt) where T : class;
        Task<bool> ValidateConfiguration();
    }
}
