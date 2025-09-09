namespace SkyGuard.Core.DTOs
{
    public class ReportStatisticsDto
    {
        public int TotalIncidents { get; set; }
        public int PendingIncidents { get; set; }
        public int CompletedIncidents { get; set; }
        public int CriticalPriority { get; set; }
        public int HighPriority { get; set; }
        public int MediumPriority { get; set; }
        public int LowPriority { get; set; }
        public int LARIncidents { get; set; }
        public int SARIncidents { get; set; }
        public Dictionary<string, int> IncidentsByClassification { get; set; }
        public Dictionary<string, int> MonthlyTrends { get; set; }
    }
}
