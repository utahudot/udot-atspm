namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    public class DecodeEventsConfiguration
    {
        /// <summary>
        /// Path to local directory where event logs are saved
        /// </summary>
        public string Path { get; set; } = System.IO.Path.GetTempPath();
    }
}
