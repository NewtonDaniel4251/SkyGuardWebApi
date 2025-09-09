namespace SkyGuard.Core.DTOs
{
    public class AuditLogFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? UserId { get; set; }
        public string Action { get; set; }
        public string Resource { get; set; }
        public bool? Success { get; set; }
    }
}
