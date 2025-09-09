namespace SkyGuard.Core.DTOs
{
    public class HotspotAnalysisDto
    {
        public string Area { get; set; }
        public int IncidentCount { get; set; }
        public string RiskLevel { get; set; }
        public string Trend { get; set; }
        public List<Coordinate> Coordinates { get; set; }
    }
}
