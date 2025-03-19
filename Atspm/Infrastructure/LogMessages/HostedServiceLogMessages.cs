#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.LogMessages/HostedServiceLogMessages.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

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
                { "service", serviceName },
                { "commandline", Environment.CommandLine}
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
