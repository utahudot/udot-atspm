namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    public class EventListenerConfiguration
    {
        /// <summary>
        /// The URL of your DataApi ingest endpoint, e.g. "https://dataapi:5001/"
        /// </summary>
        public string ApiBaseUrl { get; set; } = default!;
        
        /// <summary>
        /// The URL of your DataApi ingest endpoint, e.g. "https://dataapi:5001/"
        /// </summary>
        public string ApiEndPoint { get; set; } = default!;

        /// <summary>
        /// How many events to buffer before POSTing.
        /// </summary>
        public int BatchSize { get; set; } = 50_000;

        /// <summary>
        /// The UDP port to listen on.
        /// </summary>
        public int UdpPort { get; set; } = 10088;

        /// <summary>
        /// The UDP port to listen on.
        /// </summary>
        public int threads { get; set; } = 50;
    }
}
