using SkyGuard.Core.Enums;
using SkyGuard.Core.Models;
using SkyGuard.Core.Services;
using SkyGuard.Infrastructure.Respositories;

namespace SkyGuard.Infrastructure.Services
{
    public class IncidentService : IIncidentService
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;

        public IncidentService(
            IIncidentRepository incidentRepository,
            IUserRepository userRepository,
            IEmailService emailService)
        {
            _incidentRepository = incidentRepository;
            _userRepository = userRepository;
            _emailService = emailService;
        }

        public async Task<Incident> GetByIdAsync(Guid id)
        {
            return await _incidentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Incident>> GetByStatusAsync(IncidentStatus status)
        {
            return await _incidentRepository.GetByStatusAsync(status);
        }

        public async Task<IEnumerable<Incident>> GetByAssignedUserAsync(Guid userId)
        {
            return await _incidentRepository.GetByAssignedUserAsync(userId);
        }

        public async Task<IEnumerable<Incident>> GetByReportedUserAsync(Guid userId)
        {
            return await _incidentRepository.GetByReportedUserAsync(userId);
        }

        public async Task<IEnumerable<Incident>> GetFilteredIncidents(
            DateTime? fromDate,
            DateTime? toDate,
            AreaType? area,
            IncidentPriority? priority,
            IncidentStatus? status)
        {
            return await _incidentRepository.GetFilteredAsync(fromDate, toDate, area, priority, status);
        }

        public async Task AddAsync(Incident incident)
        {
            await _incidentRepository.AddAsync(incident);
        }

        public async Task UpdateAsync(Incident incident)
        {
            await _incidentRepository.UpdateAsync(incident);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _incidentRepository.DeleteAsync(id);
        }

        public async Task AssignIncidentAsync(Guid incidentId, Guid securityUserId)
        {
            var incident = await _incidentRepository.GetByIdAsync(incidentId);
            if (incident == null)
                throw new ArgumentException("Incident not found");

            var securityUser = await _userRepository.GetByIdAsync(securityUserId);
            if (securityUser == null || securityUser.Role != UserRole.SecurityTeam)
                throw new ArgumentException("Invalid security user");

            incident.AssignedToId = securityUser.Id;
            incident.Status = IncidentStatus.InProgress;
            incident.ReportedToSecurityAt = DateTime.UtcNow;

            await _incidentRepository.UpdateAsync(incident);

            // Send email notification
            await _emailService.SendIncidentAssignedEmail(securityUser, incident);
        }

        public async Task UpdateIncidentStatusAsync(Guid incidentId, IncidentStatus status)
        {
            var incident = await _incidentRepository.GetByIdAsync(incidentId);
            if (incident == null)
                throw new ArgumentException("Incident not found");

            incident.Status = status;
            await _incidentRepository.UpdateAsync(incident);
        }
    }
}
