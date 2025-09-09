using Microsoft.AspNetCore.Http;
using SkyGuard.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace SkyGuard.Core.DTOs
{
    public class CreateSecurityResponseDto
    {
        [Required]
        public Guid IncidentId { get; set; }

        [Required]
        public string notes { get; set; }

        public string ActionTaken { get; set; }

        [Required]
        public string Confirmation { get; set; }

        [Required]
        public IFormFile InterventionImageFile { get; set; }
    }

}
