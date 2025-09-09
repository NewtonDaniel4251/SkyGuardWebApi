using Microsoft.AspNetCore.Mvc;
using SkyGuard.Core.DTOs;
using SkyGuard.Core.Services;

namespace SkyGuard.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditController : ControllerBase
    {
        private readonly IAuditLogService _service;

        public AuditController(IAuditLogService service)
        {
            _service = service;
        }

        [HttpPost("log")]
        public async Task<IActionResult> CreateAuditLog([FromBody] CreateAuditLogRequest request)
        {
            var ip = GetClientIp(HttpContext); 
            var userAgent = Request.Headers["User-Agent"].ToString();

            await _service.CreateAuditLogAsync(request, ip, userAgent);

            return Ok();
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogFilter filter)
        {
            var logs = await _service.GetAuditLogsAsync(filter);
            return Ok(logs);
        }

        private string GetClientIp(HttpContext context)
        {
            var remoteIp = context.Connection.RemoteIpAddress?.ToString();

            if (remoteIp == "::1" || remoteIp == "127.0.0.1")
            {
                // fallback: get actual LAN IP of the host machine
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                var lanIp = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                return lanIp?.ToString() ?? remoteIp;
            }

            return remoteIp;
        }

    }
}
