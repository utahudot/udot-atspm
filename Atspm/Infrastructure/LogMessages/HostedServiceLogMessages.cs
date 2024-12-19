using Google.Cloud.Diagnostics.Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Utah.Udot.Atspm.Infrastructure.LogMessages
{
    /// <summary>
    /// Log messages for <see cref="IHostedService"/> implementations
    /// </summary>
    public partial class HostedServiceLogMessages
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Log messages for <see cref="IHostedService"/> implementations
        /// </summary>
        /// <param name="logger">Abstract logging providers</param>
        /// <param name="serviceName">Name of <see cref="IHostedService"/></param>
        public HostedServiceLogMessages(ILogger logger, string serviceName)
        {
            _logger = logger.WithAddedLabels(new Dictionary<string, string>()
            {
                { "service", serviceName }
            });
        }

        /// <summary>
        /// Log message for starting service
        /// </summary>
        /// <param name="serviceName"></param>
        [LoggerMessage(EventId = 101, EventName = "Starting hosted service", Level = LogLevel.Information, Message = "Starting {serviceName}")]
        public partial void StartingService(string serviceName);

        /// <summary>
        /// Log message for completing service
        /// </summary>
        /// <param name="serviceName">Name of <see cref="IHostedService"/></param>
        /// <param name="elapsedTime">Elapsed time of sompleted service</param>
        [LoggerMessage(EventId = 102, EventName = "Completing hosted service", Level = LogLevel.Information, Message = "Completing {serviceName} -- {elapsedTime}")]
        public partial void CompletingService(string serviceName, TimeSpan elapsedTime);

        /// <summary>
        /// Log message for stopping service
        /// </summary>
        /// <param name="serviceName">Name of <see cref="IHostedService"/></param>
        [LoggerMessage(EventId = 103, EventName = "Stopping hosted service", Level = LogLevel.Information, Message = "Stopping {serviceName}")]
        public partial void StoppingService(string serviceName);

        /// <summary>
        /// Log message for starting service cancelled
        /// </summary>
        /// <param name="serviceName">Name of <see cref="IHostedService"/></param>
        [LoggerMessage(EventId = 104, EventName = "Starting cancelled", Level = LogLevel.Debug, Message = "Starting cancelled on {serviceName}")]
        public partial void StartingCancelled(string serviceName);

        /// <summary>
        /// Log message for stopping service cancelled
        /// </summary>
        /// <param name="serviceName">Name of <see cref="IHostedService"/></param>
        [LoggerMessage(EventId = 105, EventName = "Stopping cancelled", Level = LogLevel.Debug, Message = "Stopping cancelled on {serviceName}")]
        public partial void StoppingCancelled(string serviceName);
    }
}
