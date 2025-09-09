using SkyGuard.Core.DTOs;
using SkyGuard.Core.Enums;
using SkyGuard.Core.Models;

namespace SkyGuard.Core.Services
{
    public interface IReportService
    {
        Task<ReportStatisticsDto> GetStatistics(
            DateTime? fromDate,
            DateTime? toDate,
            AreaType? area);

        Task<IEnumerable<Incident>> GenerateReport(
            DateTime? fromDate,
            DateTime? toDate,
            AreaType? area,
            IncidentPriority? priority,
            IncidentStatus? status);

        Task<byte[]> GeneratePdfReport(IEnumerable<Incident> incidents);
        Task<byte[]> GenerateExcelReport(IEnumerable<Incident> incidents);
    }
}
