using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyGuard.Core.DTOs;
using SkyGuard.Core.Enums;
using SkyGuard.Core.Models;
using SkyGuard.Core.Services;
using SkyGuard.Infrastructure.Respositories;
using System.Security.Claims;

namespace SkyGuard.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class IncidentsController : ControllerBase
    {
        private readonly IIncidentService _incidentService;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;

        public IncidentsController(IIncidentService incidentService, IUserRepository userRepository, IEmailService emailService)
        {
            _incidentService = incidentService;
            _userRepository = userRepository;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<IncidentDto>>> GetIncidents(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string? area,
            [FromQuery] string? priority,
            [FromQuery] string? status)
        {
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            AreaType? areaType = null;
            if (Enum.TryParse<AreaType>(area, out var parsedArea))
                areaType = parsedArea;

            IncidentPriority? priorityType = null;
            if (Enum.TryParse<IncidentPriority>(priority, out var parsedPriority))
                priorityType = parsedPriority;

            IncidentStatus? statusType = null;
            if (Enum.TryParse<IncidentStatus>(status, out var parsedStatus))
                statusType = parsedStatus;

            IEnumerable<Incident> incidents;

            if (currentUserRole == UserRole.Manager.ToString())
            {
                incidents = await _incidentService.GetFilteredIncidents(
                    fromDate, toDate, areaType, priorityType, statusType);
            }
            else if (currentUserRole == UserRole.SecurityTeam.ToString())
            {
                incidents = await _incidentService.GetByAssignedUserAsync(currentUserId);
            }
            else // UAS Coordinator
            {
                incidents = await _incidentService.GetByReportedUserAsync(currentUserId);
            }

            return Ok(incidents.Select(i => MapToDto(i)));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IncidentDetailsDto>> GetIncident(Guid id)
        {
            var incident = await _incidentService.GetByIdAsync(id);
            if (incident == null) return NotFound();

            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            // Check permissions
            if (currentUserRole != UserRole.Manager.ToString() &&
                incident.ReportedById != currentUserId &&
                incident.AssignedToId != currentUserId)
                return Forbid();

            return Ok(MapToDetailsDto(incident));
        }

        [HttpPost]
        [Authorize(Roles = "UASCoordinator")]
        public async Task<ActionResult<IncidentDto>> CreateIncident([FromBody]CreateIncidentDto incidentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var reportedById = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var getSecurityUser = await _userRepository.GetByEmailAsync(incidentDto.AssignedTo);
            if (getSecurityUser == null || getSecurityUser.Role != UserRole.SecurityTeam)
                return BadRequest("Invalid security user");

            var priorityString = incidentDto.Priority;

            if (!Enum.TryParse<IncidentPriority>(priorityString, ignoreCase: true, out var parsedPriority))
            {
                return BadRequest("Invalid priority specified");
            }
            var areaString = incidentDto.Area;

            if (!Enum.TryParse<AreaType>(areaString, ignoreCase: true, out var parsedArea))
            {
                return BadRequest("Invalid area specified");
            }

            var incident = new Incident
            {
                Title = incidentDto.Title,
                Description = incidentDto.Description,
                Priority = parsedPriority,
                Area = parsedArea,
                Latitude = incidentDto.Latitude,
                Longitude = incidentDto.Longitude,
                PathLine = incidentDto.PipelineLocation,
                ImageLink = incidentDto.ImageSharePointUrl,
                VideoLink = incidentDto.VideoSharePointUrl,
                ReportedById = reportedById,
                ReportedAt = Convert.ToDateTime(incidentDto.ReportedDate)
            };

            await _incidentService.AddAsync(incident);

            /// Assign Incident to Security Person
            var getIncident = await _incidentService.GetByIdAsync(incident.Id);            

            // Update incident
            incident.AssignedToId = getSecurityUser.Id;
            incident.Status = IncidentStatus.InProgress;
            incident.ReportedToSecurityAt = DateTime.UtcNow;

            await _incidentService.UpdateAsync(incident);

            // Send email notification to security user
            await _emailService.SendIncidentAssignedEmail(getSecurityUser, incident);

            return CreatedAtAction(nameof(GetIncident), new { id = incident.Id }, MapToDto(incident));
        }

  

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, UpdateStatusDto statusDto)
        {
            var incident = await _incidentService.GetByIdAsync(id);
            if (incident == null) return NotFound();

            var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Check permissions
            if (currentUserRole == UserRole.UASCoordinator.ToString() &&
                incident.ReportedById != currentUserId)
                return Forbid();

            if (currentUserRole == UserRole.SecurityTeam.ToString() &&
                incident.AssignedToId != currentUserId)
                return Forbid();

            incident.Status = statusDto.Status;
            await _incidentService.UpdateAsync(incident);

            return NoContent();
        }

        [HttpGet("security-team")]
        [Authorize(Roles = "UASCoordinator")]
        public async Task<IActionResult> GetAllSecurityMembers()
        {
            var allUsers = await _userRepository.GetAllAsync();
            var filterSecurityUsers = allUsers.Where(x => x.Role == UserRole.SecurityTeam);
            if (filterSecurityUsers.Any())
            return Ok(filterSecurityUsers);
            else return NoContent();
        }

        // Helper methods to map to DTOs
        private IncidentDto MapToDto(Incident incident) => new()
        {
            Id = incident.Id,
            Title = incident.Title,
            Priority = incident.Priority.ToString(),
            Status = incident.Status.ToString(),
            Area = incident.Area.ToString(),
            ReportedAt = incident.ReportedAt,
            ReportedBy = incident.ReportedBy?.Name ?? "Unknown",
            ReportedToSecurityAt = incident.ReportedToSecurityAt ?? DateTime.MinValue,
            Latitude = incident.Latitude,
            Longitude = incident.Longitude,
            PipelineLocation = incident.PathLine,
            ImageSharePointUrl = incident.ImageLink,
            VideoSharePointUrl = incident.VideoLink,
            Description = incident.Description
        };

        private IncidentDetailsDto MapToDetailsDto(Incident incident) => new()
        {
            Id = incident.Id,
            Title = incident.Title,
            Description = incident.Description,
            Priority = incident.Priority.ToString(),
            Status = incident.Status.ToString(),
            Area = incident.Area.ToString(),
            Latitude = incident.Latitude,
            Longitude = incident.Longitude,
            PathLine = incident.PathLine,
            ImageSharePointUrl = incident.ImageLink,
            VideoSharePointUrl = incident.VideoLink,
            ReportedAt = incident.ReportedAt,
            ReportedToSecurityAt = incident.ReportedToSecurityAt,
            ReportedBy = incident.ReportedBy != null ? new UserDto
            {
                Id = incident.ReportedBy.Id,
                Name = incident.ReportedBy.Name ?? "Unknown",
                Email = incident.ReportedBy.Email ?? string.Empty
            } : new UserDto { Name = "Unknown" }, 
            AssignedTo = incident.AssignedTo != null ? new UserDto
            {
                Id = incident.AssignedTo.Id,
                Name = incident.AssignedTo.Name ?? "Unassigned",
                Email = incident.AssignedTo.Email ?? string.Empty
            } : null,
            Response = incident.Response != null ? new SecurityResponseDto
            {
                ActionTaken = incident.Response.ActionTaken,
                notes = incident.Response.AdditionalComments,
                Confirmation = MapClassificationReturn(incident.Response.Classification),
                InterventionImageUrl = incident.Response.InterventionImagePath,
                RespondedAt = incident.Response.RespondedAt,
                RespondedBy = incident.Response.RespondedBy != null ? new UserDto
                {
                    Id = incident.Response.RespondedBy.Id,
                    Name = incident.Response.RespondedBy.Name ?? "Unknown",
                    Email = incident.Response.RespondedBy.Email ?? string.Empty
                } : new UserDto { Name = "Unknown" }
            } : null
        };

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
