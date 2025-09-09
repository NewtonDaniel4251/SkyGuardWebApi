namespace SkyGuard.Core.DTOs
{
    public class CreateAuditLogRequest
    {
        public string Action { get; set; }
        public string Resource { get; set; }
        public string ResourceId { get; set; }
        public string Details { get; set; }
        public bool Success { get; set; }

        public Guid UserId { get; set; }
    }
}
