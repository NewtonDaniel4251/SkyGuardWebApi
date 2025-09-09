using Microsoft.EntityFrameworkCore;
using SkyGuard.Core.Enums;
using SkyGuard.Core.Models;
using SkyGuard.Infrastructure.Data;

namespace SkyGuard.Infrastructure.Respositories
{
    public class IncidentRepository : BaseRepository<Incident>, IIncidentRepository
    {
        public IncidentRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Incident>> GetByStatusAsync(IncidentStatus status)
        {
            return await _context.Incidents
                .Where(i => i.Status == status)
                .Include(i => i.ReportedBy)
                .Include(i => i.AssignedTo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Incident>> GetByAssignedUserAsync(Guid userId)
        {
            return await _context.Incidents
                .Where(i => i.AssignedToId == userId)
                .Include(i => i.ReportedBy)
                .ToListAsync();
        }

        public async Task<IEnumerable<Incident>> GetByReportedUserAsync(Guid userId)
        {
            return await _context.Incidents
                .Where(i => i.ReportedById == userId)
                .Include(i => i.AssignedTo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Incident>> GetFilteredAsync(DateTime? fromDate, DateTime? toDate,
            AreaType? area, IncidentPriority? priority, IncidentStatus? status)
        {
            var query = _context.Incidents.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(i => i.ReportedAt >= fromDate);

            if (toDate.HasValue)
                query = query.Where(i => i.ReportedAt <= toDate);

            if (area.HasValue)
                query = query.Where(i => i.Area == area);

            if (priority.HasValue)
                query = query.Where(i => i.Priority == priority);

            if (status.HasValue)
                query = query.Where(i => i.Status == status);

            return await query
                .Include(i => i.ReportedBy)
                .Include(i => i.AssignedTo)
                .ToListAsync();
        }

        public async Task<User> GetReportedByAsync(Guid incidentId)
        {
            return await _context.Incidents
                .Where(i => i.Id == incidentId)
                .Select(i => i.ReportedBy)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Incident>> GetRecentIncidentsAsync(int count)
        {
            try
            {
                return await _context.Incidents
                    .AsNoTracking()
                    .OrderByDescending(i => i.ReportedAt)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<Incident>> GetLastMonthsIncidentsAsync(int months)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddMonths(-months);

                return await _context.Incidents
                    .AsNoTracking()
                    .Where(i => i.ReportedAt >= cutoffDate)
                    .OrderByDescending(i => i.ReportedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<Incident>> GetLastYearsIncidentsAsync()
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddYears(-1);

                return await _context.Incidents
                    .AsNoTracking()
                    .Where(i => i.ReportedAt >= cutoffDate)
                    .OrderByDescending(i => i.ReportedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<List<Incident>> GetAllIncidentsAsync()
        {
            try
            {
                return await _context.Incidents
                    .AsNoTracking()
                    .OrderByDescending(i => i.ReportedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<Incident>> GetIncidentsByTimeRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.Incidents
                    .AsNoTracking()
                    .Where(i => i.ReportedAt >= startDate && i.ReportedAt <= endDate)
                    .OrderByDescending(i => i.ReportedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
