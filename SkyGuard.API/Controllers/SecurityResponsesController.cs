using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyGuard.Core.DTOs;
using SkyGuard.Core.Enums;
using SkyGuard.Core.Services;
using System.Security.Claims;

namespace SkyGuard.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SecurityTeam")]
    public class SecurityResponsesController : ControllerBase
    {
        private readonly ISecurityResponseService _responseService;
        private readonly IIncidentService _incidentService;
        private readonly IFileStorageService _fileStorage;

        public SecurityResponsesController(
            ISecurityResponseService responseService,
            IIncidentService incidentService,
            IFileStorageService fileStorage)
        {
            _responseService = responseService;
            _incidentService = incidentService;
            _fileStorage = fileStorage;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<SecurityResponseDto>> CreateResponse(
            [FromForm] CreateSecurityResponseDto responseDto)
        {
            var response = await _responseService.SubmitResponseAsync(responseDto, Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!));           
            if (response == null)
            {
                return BadRequest("Failed to create security response.");
            }
            return Ok(response);
        }

        [HttpGet("{incidentId}")]
        public async Task<ActionResult<SecurityResponseDto>> GetResponse(Guid incidentId)
        {
            var response = await _responseService.GetByIncidentIdAsync(incidentId);
            if (response == null) return NotFound();

            var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (response.RespondedById != currentUserId &&
                User.FindFirst(ClaimTypes.Role)?.Value != UserRole.Manager.ToString())
                return Forbid();

            return Ok(new SecurityResponseDto
            {
                Id = response.Id,
                ActionTaken = response.ActionTaken,
                notes = response.AdditionalComments,
                Confirmation = MapClassificationReturn(response.Classification),
                InterventionImageUrl = response.InterventionImagePath,
                RespondedAt = response.RespondedAt,
                RespondedBy = new UserDto
                {
                    Id = response.RespondedBy.Id,
                    Name = response.RespondedBy.Name,
                    Email = response.RespondedBy.Email
                }
            });
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
