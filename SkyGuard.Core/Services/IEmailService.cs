using SkyGuard.Core.Models;

namespace SkyGuard.Core.Services
{
    public interface IEmailService
    {
        Task SendIncidentAssignedEmail(User securityUser, Incident incident);
        Task SendResponseSubmittedEmail(string recipientEmail, string recipientName, Guid incidentId);
    }
}
