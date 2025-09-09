using SkyGuard.Core.DTOs;
using SkyGuard.Core.Enums;
using SkyGuard.Core.Models;
using SkyGuard.Core.Services;
using SkyGuard.Infrastructure.Respositories;
using System.Security.Claims;

namespace SkyGuard.Infrastructure.Services
{
    public class SecurityResponseService : ISecurityResponseService
    {
        private readonly ISecurityResponseRepository _responseRepository;
        private readonly IIncidentRepository _incidentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFileStorageService _fileStorage;
        private readonly IEmailService _emailService;

        public SecurityResponseService(
            ISecurityResponseRepository responseRepository,
            IIncidentRepository incidentRepository,
            IFileStorageService fileStorage,
            IEmailService emailService,
            IUserRepository userRepository)
        {
            _responseRepository = responseRepository;
            _incidentRepository = incidentRepository;
            _fileStorage = fileStorage;
            _emailService = emailService;
            _userRepository = userRepository;
        }

        public async Task<SecurityResponse> GetByIncidentIdAsync(Guid incidentId)
        {
            return await _responseRepository.GetByIncidentIdAsync(incidentId);
        }

        public async Task AddAsync(SecurityResponse response)
        {
            await _responseRepository.AddAsync(response);
        }

        public async Task UpdateAsync(SecurityResponse response)
        {
            await _responseRepository.UpdateAsync(response);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _responseRepository.DeleteAsync(id);
        }

       public async Task<SecurityResponseDto> SubmitResponseAsync(CreateSecurityResponseDto responseDto, Guid userId)
        {
            if (responseDto == null)
                throw new ArgumentNullException(nameof(responseDto));

            if (responseDto.IncidentId == Guid.Empty)
                throw new ArgumentException("Incident ID is required", nameof(responseDto.IncidentId));

            var incident = await _incidentRepository.GetByIdAsync(responseDto.IncidentId);
            if (incident == null)
                throw new ArgumentException("Incident not found");

            if (incident.AssignedToId != userId)
                throw new UnauthorizedAccessException("You are not assigned to this incident");

            if (incident.Status != IncidentStatus.InProgress)
                throw new InvalidOperationException("Incident is not in progress");

            if (responseDto.InterventionImageFile == null || responseDto.InterventionImageFile.Length == 0)
                throw new ArgumentException("Intervention image is required");

            var user = await _userRepository.GetByIdAsync(userId);

            // Upload intervention image
            var filePath = await _fileStorage.SaveFile(responseDto.InterventionImageFile);

            IncidentClassification category = MapClassification(responseDto.Confirmation);

            var response = new SecurityResponse
            {
                IncidentId = responseDto.IncidentId,
                ActionTaken = responseDto.ActionTaken,
                AdditionalComments = responseDto.notes,
                Classification = category,
                InterventionImagePath = filePath,
                RespondedById = userId
            };

            await _responseRepository.AddAsync(response);

            // Update incident status
            incident.Status = IncidentStatus.Completed;
            await _incidentRepository.UpdateAsync(incident);

            // Notify the reporter
            var reporter = await _incidentRepository.GetReportedByAsync(responseDto.IncidentId);
            await _emailService.SendResponseSubmittedEmail(
                reporter.Email,
                reporter.Name,
                incident.Id);


            return new SecurityResponseDto
            {
                Id = response.Id,
                ActionTaken = response.ActionTaken,
                notes = response.AdditionalComments,
                Confirmation = MapClassificationReturn(response.Classification),
                InterventionImageUrl = response.InterventionImagePath,
                RespondedAt = response.RespondedAt,
                RespondedBy = new UserDto
                {
                    Id = userId,
                    Name = user.Name,
                    Email = user.Email
                }
            };
        }

        private static IncidentClassification MapClassification(string classification)
        {
            return classification switch
            {
                "Active IR Point" => IncidentClassification.ActiveIRPoint,
                "Active IC Point" => IncidentClassification.ActiveICPoint,
                "Active Leak Point" => IncidentClassification.ActiveLeakPoint,
                "Inactive" => IncidentClassification.InactiveOldPoint,
                "False Positive" => IncidentClassification.FalsePositive,
                "Wrong Coordinate" => IncidentClassification.WrongCoordinate,
                "Old IR Point" => IncidentClassification.WrongCoordinate,
                _ => throw new ArgumentException("Invalid classification value")
            };
        }

        private static string MapClassificationReturn(IncidentClassification classification)
        {
            return classification switch
            {
                IncidentClassification.ActiveIRPoint => "Active IR Point",
                IncidentClassification.ActiveICPoint => "Active IC Point",
                IncidentClassification.ActiveLeakPoint => "Active Leak Point",
                IncidentClassification.InactiveOldPoint => "Inactive",
                IncidentClassification.FalsePositive => "False Positive",
                IncidentClassification.WrongCoordinate => "Wrong Coordinate",
                IncidentClassification.OldIRPoint => "Old IR Point",
                _ => throw new ArgumentException("Invalid classification value")
            };
        }
    }
}
