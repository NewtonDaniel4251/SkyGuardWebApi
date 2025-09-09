using SkyGuard.Core.Enums;

namespace SkyGuard.Core.DTOs
{
    public class SecurityResponseDto
    {
        public Guid Id { get; set; }
        public string ActionTaken { get; set; }
        public string notes { get; set; }
        public string Confirmation { get; set; }
        public string InterventionImageUrl { get; set; }
        public DateTime RespondedAt { get; set; }
        public UserDto RespondedBy { get; set; }
    }
}
