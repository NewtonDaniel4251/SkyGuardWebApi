using SkyGuard.Core.DTOs;
using SkyGuard.Core.Models;

namespace SkyGuard.Core.Services
{
    public interface IAuditLogService
    {
        Task CreateAuditLogAsync(CreateAuditLogRequest request,  string ip, string userAgent);
        Task<IEnumerable<AuditLogEntry>> GetAuditLogsAsync(AuditLogFilter filter);
    }
}
