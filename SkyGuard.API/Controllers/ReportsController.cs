using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyGuard.Core.DTOs;
using SkyGuard.Core.Enums;
using SkyGuard.Core.Services;

namespace SkyGuard.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Manager")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<ReportStatisticsDto>> GetStatistics(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string? area)
        {
            AreaType? areaType = null;
            if (Enum.TryParse<AreaType>(area, out var parsedArea))
                areaType = parsedArea;

            var stats = await _reportService.GetStatistics(fromDate, toDate, areaType);
            return Ok(stats);
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportReport(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string? area,
            [FromQuery] string? priority,
            [FromQuery] string? status,
            [FromQuery] string format = "pdf")
        {
            AreaType? areaType = null;
            if (Enum.TryParse<AreaType>(area, out var parsedArea))
                areaType = parsedArea;

            IncidentPriority? priorityType = null;
            if (Enum.TryParse<IncidentPriority>(priority, out var parsedPriority))
                priorityType = parsedPriority;

            IncidentStatus? statusType = null;
            if (Enum.TryParse<IncidentStatus>(status, out var parsedStatus))
                statusType = parsedStatus;

            var reportData = await _reportService.GenerateReport(
                fromDate, toDate, areaType, priorityType, statusType);

            if (format.ToLower() == "pdf")
            {
                var pdfBytes = _reportService.GeneratePdfReport(reportData);
                return File(await pdfBytes.ConfigureAwait(false), "application/pdf", $"IncidentReport_{DateTime.UtcNow:yyyyMMdd}.pdf");
            }
            else if (format.ToLower() == "excel")
            {
                var excelBytes = _reportService.GenerateExcelReport(reportData);
                return File(await excelBytes.ConfigureAwait(false),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"IncidentReport_{DateTime.UtcNow:yyyyMMdd}.xlsx");
            }
            else
            {
                return BadRequest("Invalid format. Supported formats: pdf, excel");
            }
        }
    }
}
