namespace SpeedManagementDataDownloader.Common.Dtos
{
    public class RouteEntityWithSpeedAndAlternateIdentifier
    {
        public int RouteId { get; set; }
        public int EntityId { get; set; }
        public string EntityType { get; set; }
        public string SourceId { get; set; }
        public int SpeedLimit { get; set; }
        public string AlternateIdentifier { get; set; }
    }
}
