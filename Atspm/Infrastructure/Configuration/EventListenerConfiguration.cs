namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    public class EventListenerConfiguration
    {
        /// <summary>
        /// The URL of your DataApi ingest endpoint, e.g. "https://dataapi:5001/"
        /// </summary>
        public string ApiBaseUrl { get; set; } = default!;

        /// <summary>
        /// How many events to buffer before POSTing.
        /// </summary>
        public int BatchSize { get; set; } = 50_000;

        /// <summary>
        /// Maximum seconds to wait before flushing a partial batch.
        /// </summary>
        public int IntervalSeconds { get; set; } = 30;

        /// <summary>
        /// The UDP port to listen on.
        /// </summary>
        public int UdpPort { get; set; } = 10088;

        /// <summary>
        /// The TCP port to listen on.
        /// </summary>
        public int TcpPort { get; set; } = 10088;
    }
}
