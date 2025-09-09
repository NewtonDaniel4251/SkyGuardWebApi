using System.ComponentModel.DataAnnotations;

namespace SkyGuard.Core.DTOs
{
    public class AssignIncidentDto
    {
        [Required]
        public Guid SecurityUserId { get; set; }
    }
}
