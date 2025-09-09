namespace SkyGuard.Core.DTOs
{
    public class PatternAnalysisDto
    {
        public List<int> PeakHours { get; set; }
        public string SeasonalTrend { get; set; }
        public string WeeklyPattern { get; set; }
        public List<string> Anomalies { get; set; }
    }
    public class Coordinate
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

}
