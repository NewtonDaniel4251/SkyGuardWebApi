using SkyGuard.Core.DTOs;
using SkyGuard.Core.Models;

namespace SkyGuard.Core.Services
{
    public interface ISecurityResponseService
    {
        Task<SecurityResponse> GetByIncidentIdAsync(Guid incidentId);
        Task AddAsync(SecurityResponse response);
        Task UpdateAsync(SecurityResponse response);
        Task DeleteAsync(Guid id);
        Task<SecurityResponseDto> SubmitResponseAsync(CreateSecurityResponseDto responseDto, Guid userId);
    }
}
