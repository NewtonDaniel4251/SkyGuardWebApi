using SkyGuard.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace SkyGuard.Core.DTOs
{
    public class UpdateStatusDto
    {
        [Required]
        public IncidentStatus Status { get; set; }
    }
}
