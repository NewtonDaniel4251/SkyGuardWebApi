using SkyGuard.Core.Enums;
using SkyGuard.Core.Models;

namespace SkyGuard.Infrastructure.Respositories
{
    public interface IIncidentRepository : IRepository<Incident>
    {
        Task<IEnumerable<Incident>> GetByStatusAsync(IncidentStatus status);
        Task<IEnumerable<Incident>> GetByAssignedUserAsync(Guid userId);
        Task<IEnumerable<Incident>> GetByReportedUserAsync(Guid userId);
        Task<IEnumerable<Incident>> GetFilteredAsync(DateTime? fromDate, DateTime? toDate,
            AreaType? area, IncidentPriority? priority, IncidentStatus? status);
        Task<User> GetReportedByAsync(Guid incidentId);
        Task<List<Incident>> GetRecentIncidentsAsync(int count);
        Task<List<Incident>> GetLastMonthsIncidentsAsync(int months);
        Task<List<Incident>> GetLastYearsIncidentsAsync();
        Task<List<Incident>> GetAllIncidentsAsync();  

    }
}
