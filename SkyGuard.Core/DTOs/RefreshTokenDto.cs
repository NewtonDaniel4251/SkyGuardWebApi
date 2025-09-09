using System.ComponentModel.DataAnnotations;

namespace SkyGuard.Core.DTOs
{
    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
