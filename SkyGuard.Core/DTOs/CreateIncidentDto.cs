using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace SkyGuard.Core.DTOs
{
    public class CreateIncidentDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Priority { get; set; }

        [Required]
        public string Area { get; set; }

        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Range(-180, 180)]
        public double Longitude { get; set; }

        [Required]
        [StringLength(100)]
        public string PipelineLocation { get; set; }

        [Url]
        public string? ImageSharePointUrl { get; set; }

        [Url]
        public string? VideoSharePointUrl { get; set; }

        [Required]
        public DateTime ReportedDate { get; set; }

        [Required]
        public string AssignedTo { get; set; }
    }
}
