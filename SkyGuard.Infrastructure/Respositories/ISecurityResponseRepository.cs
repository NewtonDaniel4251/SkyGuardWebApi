using SkyGuard.Core.Models;

namespace SkyGuard.Infrastructure.Respositories
{
    public interface ISecurityResponseRepository : IRepository<SecurityResponse>
    {
        Task<SecurityResponse> GetByIncidentIdAsync(Guid incidentId);
        Task<List<SecurityResponse>> GetRecentResponsesAsync(int count);
        Task<List<SecurityResponse>> GetByIncidentIdAsync(string incidentId);
    }
}
