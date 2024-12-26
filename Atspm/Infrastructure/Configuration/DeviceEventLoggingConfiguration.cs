using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.ValueObjects;

namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    /// <summary>
    /// Configuration options for device event logging
    /// </summary>
    public class DeviceEventLoggingConfiguration
    {
        /// <summary>
        /// Path to local directory where event logs are saved
        /// </summary>
        public string Path { get; set; } = System.IO.Path.GetTempPath();

        /// <summary>
        /// Batch size of <see cref="EventLogModelBase"/> objects when saving to repository
        /// </summary>
        public int BatchSize { get; set; } = 50000;

        /// <summary>
        /// Amount of processes that can be run in parallel
        /// </summary>
        public int ParallelProcesses { get; set; } = 50;

        /// <inheritdoc cref="DeviceEventLoggingQueryOptions"/>
        public DeviceEventLoggingQueryOptions DeviceEventLoggingQueryOptions { get; set; } = new DeviceEventLoggingQueryOptions();
    }
}
