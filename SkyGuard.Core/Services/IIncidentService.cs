using SkyGuard.Core.Enums;
using SkyGuard.Core.Models;

namespace SkyGuard.Core.Services
{
    public interface IIncidentService
    {
        Task<Incident> GetByIdAsync(Guid id);
        Task<IEnumerable<Incident>> GetByStatusAsync(IncidentStatus status);
        Task<IEnumerable<Incident>> GetByAssignedUserAsync(Guid userId);
        Task<IEnumerable<Incident>> GetByReportedUserAsync(Guid userId);
        Task<IEnumerable<Incident>> GetFilteredIncidents(
            DateTime? fromDate,
            DateTime? toDate,
            AreaType? area,
            IncidentPriority? priority,
            IncidentStatus? status);
        Task AddAsync(Incident incident);
        Task UpdateAsync(Incident incident);
        Task DeleteAsync(Guid id);
        Task AssignIncidentAsync(Guid incidentId, Guid securityUserId);
        Task UpdateIncidentStatusAsync(Guid incidentId, IncidentStatus status);
    }
}
