using SkyGuard.Core.DTOs;
using SkyGuard.Core.Models;

namespace SkyGuard.Infrastructure.Respositories
{
    public interface IAuditLogRepository
    {
        Task AddAsync(AuditLogEntry log);
        Task<IEnumerable<AuditLogEntry>> GetLogsAsync(AuditLogFilter filter);
    }
}
