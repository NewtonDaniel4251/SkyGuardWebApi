using SkyGuard.Core.DTOs;
using SkyGuard.Core.Models;
using SkyGuard.Core.Services;
using SkyGuard.Infrastructure.Respositories;

namespace SkyGuard.Infrastructure.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _repository;
        private readonly IUserRepository _userRepository;


        public AuditLogService(IAuditLogRepository repository, IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
        }

        public async Task CreateAuditLogAsync(CreateAuditLogRequest request, string ip, string userAgent)
        {
            var userDetails = await _userRepository.GetByEmailAsync(request.ResourceId);
            var auditLog = new AuditLogEntry
            {
                Action = request.Action,
                Resource = request.Resource,
                ResourceId = request.ResourceId,
                Details = request.Details,
                Success = request.Success,
                UserId = userDetails.Id,
                IpAddress = ip,
                UserAgent = userAgent,
                Timestamp = DateTime.UtcNow
            };

            await _repository.AddAsync(auditLog);
        }

        public async Task<IEnumerable<AuditLogEntry>> GetAuditLogsAsync(AuditLogFilter filter)
        {
            return await _repository.GetLogsAsync(filter);
        }
    }
}
