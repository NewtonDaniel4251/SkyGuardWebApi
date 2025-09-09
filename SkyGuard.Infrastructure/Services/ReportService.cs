using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using SkyGuard.Core.DTOs;
using SkyGuard.Core.Enums;
using SkyGuard.Core.Models;
using SkyGuard.Core.Services;
using SkyGuard.Infrastructure.Data;
using SkyGuard.Infrastructure.Respositories;
using System.Text;

namespace SkyGuard.Infrastructure.Services
{
    public class ReportService : IReportService
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly AppDbContext _context;

        public ReportService(IIncidentRepository incidentRepository, AppDbContext context)
        {
            _incidentRepository = incidentRepository;
            _context = context;
        }

        public async Task<ReportStatisticsDto> GetStatistics(
            DateTime? fromDate,
            DateTime? toDate,
            AreaType? area)
        {
            var incidents = await _incidentRepository.GetFilteredAsync(fromDate, toDate, area, null, null);

            var stats = new ReportStatisticsDto
            {
                TotalIncidents = incidents.Count(),
                PendingIncidents = incidents.Count(i => i.Status == IncidentStatus.Pending),
                CompletedIncidents = incidents.Count(i => i.Status == IncidentStatus.Completed),
                CriticalPriority = incidents.Count(i => i.Priority == IncidentPriority.Critical),
                HighPriority = incidents.Count(i => i.Priority == IncidentPriority.High),
                MediumPriority = incidents.Count(i => i.Priority == IncidentPriority.Medium),
                LowPriority = incidents.Count(i => i.Priority == IncidentPriority.Low),
                LARIncidents = incidents.Count(i => i.Area == AreaType.LAR),
                SARIncidents = incidents.Count(i => i.Area == AreaType.SAR),
                IncidentsByClassification = new Dictionary<string, int>(),
                MonthlyTrends = new Dictionary<string, int>()
            };

            // Calculate incidents by classification
            var responses = await _context.SecurityResponses
                .Include(r => r.Incident)
                .Where(r => incidents.Select(i => i.Id).Contains(r.IncidentId))
                .ToListAsync();

            foreach (IncidentClassification classification in Enum.GetValues(typeof(IncidentClassification)))
            {
                stats.IncidentsByClassification[classification.ToString()] =
                    responses.Count(r => r.Classification == classification);
            }

            // Calculate monthly trends
            var startDate = fromDate ?? DateTime.UtcNow.AddYears(-1);
            var endDate = toDate ?? DateTime.UtcNow;

            for (var date = startDate; date <= endDate; date = date.AddMonths(1))
            {
                var monthYear = date.ToString("yyyy-MM");
                stats.MonthlyTrends[monthYear] = incidents
                    .Count(i => i.ReportedAt.Year == date.Year && i.ReportedAt.Month == date.Month);
            }

            return stats;
        }

        public async Task<IEnumerable<Incident>> GenerateReport(
            DateTime? fromDate,
            DateTime? toDate,
            AreaType? area,
            IncidentPriority? priority,
            IncidentStatus? status)
        {
            return await _incidentRepository.GetFilteredAsync(fromDate, toDate, area, priority, status);
        }

        public async Task<byte[]> GeneratePdfReport(IEnumerable<Incident> incidents)
        {
            // In a real implementation, you would use a library like iTextSharp or QuestPDF
            // This is a simplified version that generates a basic PDF

            var sb = new StringBuilder();
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; }");
            sb.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            sb.AppendLine("th { background-color: #f2f2f2; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<h1>SkyGuard SecureFlow Incident Report</h1>");
            sb.AppendLine($"<p>Generated on: {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")}</p>");
            sb.AppendLine($"<p>Total Incidents: {incidents.Count()}</p>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th>ID</th>");
            sb.AppendLine("<th>Title</th>");
            sb.AppendLine("<th>Priority</th>");
            sb.AppendLine("<th>Status</th>");
            sb.AppendLine("<th>Area</th>");
            sb.AppendLine("<th>Reported At</th>");
            sb.AppendLine("<th>Reported By</th>");
            sb.AppendLine("</tr>");

            foreach (var incident in incidents)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{incident.Id}</td>");
                sb.AppendLine($"<td>{incident.Title}</td>");
                sb.AppendLine($"<td>{incident.Priority}</td>");
                sb.AppendLine($"<td>{incident.Status}</td>");
                sb.AppendLine($"<td>{incident.Area}</td>");
                sb.AppendLine($"<td>{incident.ReportedAt.ToString("yyyy-MM-dd HH:mm")}</td>");
                sb.AppendLine($"<td>{incident.ReportedBy?.Name}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            // In a real implementation, you would convert this HTML to PDF
            // For now, we'll just return the HTML as bytes
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> GenerateExcelReport(IEnumerable<Incident> incidents)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage();

            var worksheet = package.Workbook.Worksheets.Add("Incidents");

            // Add headers
            worksheet.Cells[1, 1].Value = "ID";
            worksheet.Cells[1, 2].Value = "Title";
            worksheet.Cells[1, 3].Value = "Priority";
            worksheet.Cells[1, 4].Value = "Status";
            worksheet.Cells[1, 5].Value = "Area";
            worksheet.Cells[1, 6].Value = "Reported At";
            worksheet.Cells[1, 7].Value = "Reported By";
            worksheet.Cells[1, 8].Value = "Location";
            worksheet.Cells[1, 9].Value = "Description";

            // Format headers
            using (var range = worksheet.Cells[1, 1, 1, 9])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            // Add data
            var row = 2;
            foreach (var incident in incidents)
            {
                worksheet.Cells[row, 1].Value = incident.Id;
                worksheet.Cells[row, 2].Value = incident.Title;
                worksheet.Cells[row, 3].Value = incident.Priority.ToString();
                worksheet.Cells[row, 4].Value = incident.Status.ToString();
                worksheet.Cells[row, 5].Value = incident.Area.ToString();
                worksheet.Cells[row, 6].Value = incident.ReportedAt.ToString("yyyy-MM-dd HH:mm");
                worksheet.Cells[row, 7].Value = incident.ReportedBy?.Name;
                worksheet.Cells[row, 8].Value = $"{incident.Latitude}, {incident.Longitude}";
                worksheet.Cells[row, 9].Value = incident.Description;
                row++;
            }

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            return await package.GetAsByteArrayAsync();
        }
    }
}
