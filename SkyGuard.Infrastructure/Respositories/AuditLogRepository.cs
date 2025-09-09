using Microsoft.EntityFrameworkCore;
using SkyGuard.Core.DTOs;
using SkyGuard.Core.Models;
using SkyGuard.Infrastructure.Data;

namespace SkyGuard.Infrastructure.Respositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly AppDbContext _context;

        public AuditLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AuditLogEntry log)
        {
            await _context.AuditLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AuditLogEntry>> GetLogsAsync(AuditLogFilter filter)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (filter.StartDate.HasValue)
                query = query.Where(a => a.Timestamp >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(a => a.Timestamp <= filter.EndDate.Value);

            if (filter.UserId.HasValue)
                query = query.Where(a => a.UserId == filter.UserId);

            if (!string.IsNullOrEmpty(filter.Action))
                query = query.Where(a => a.Action == filter.Action);

            if (!string.IsNullOrEmpty(filter.Resource))
                query = query.Where(a => a.Resource == filter.Resource);

            if (filter.Success.HasValue)
                query = query.Where(a => a.Success == filter.Success.Value);

            return await query
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }
    }
}
