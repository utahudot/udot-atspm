#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.LogMessages/DeviceDownloaderLogMessages.cs
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
using Microsoft.Extensions.Logging;
using System.Net;

namespace Utah.Udot.Atspm.Infrastructure.LogMessages
{
    /// <summary>
    /// Log messages for <see cref="IDeviceDownloader"/> implementations
    /// </summary>
    public partial class DeviceDownloaderLogMessages
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Log messages for <see cref="IDeviceDownloader"/> implementations
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="serviceName"></param>
        /// <param name="device"></param>
        public DeviceDownloaderLogMessages(ILogger logger, string serviceName, Device device)
        {
            _logger = logger.WithAddedLabels(new Dictionary<string, string>()
            {
                { "service", serviceName },
                { "DeviceId", device.Id.ToString() },
                { "deviceIdentifier", device?.DeviceIdentifier },
                { "DeviceType", device?.DeviceType.ToString() },
                { "DeviceStatus", device?.DeviceStatus.ToString() },
                { "IPAddress", device?.Ipaddress?.ToString() },
                { "ConfigurationId", device?.DeviceConfigurationId?.ToString() },
                { "TransportProtocol", device?.DeviceConfiguration.Protocol.ToString() },
                { "Model", device?.DeviceConfiguration?.Product?.Model },
                { "LocationIdentifier", device?.Location?.LocationIdentifier },
            });
        }

        #region HostConnectionMessages

        /// <summary>
        /// Connecting to host message
        /// </summary>
        /// <param name="deviceIdentifier"></param>
        /// <param name="ip"></param>
        [LoggerMessage(EventId = 1000, EventName = "Host Connecting", Level = LogLevel.Debug, Message = "Connecting to {deviceIdentifier} at {ip}")]
        public partial void ConnectingToHostMessage(string deviceIdentifier, IPAddress ip);

        /// <summary>
        /// Connected to host message
        /// </summary>
        /// <param name="deviceIdentifier"></param>
        /// <param name="ip"></param>
        [LoggerMessage(EventId = 1001, EventName = "Host Connected", Level = LogLevel.Debug, Message = "Connected to {deviceIdentifier} at {ip}")]
        public partial void ConnectedToHostMessage(string deviceIdentifier, IPAddress ip);

        /// <summary>
        /// Connecting to host exception
        /// </summary>
        /// <param name="deviceIdentifier"></param>
        /// <param name="ip"></param>
        /// <param name="ex"></param>
        [LoggerMessage(EventId = 1002, EventName = "Host Connection Exception", Level = LogLevel.Warning, Message = "Exception connecting to {deviceIdentifier} at {ip}")]
        public partial void ConnectingToHostException(string deviceIdentifier, IPAddress ip, Exception ex = null);

        /// <summary>
        /// Not connected to host exception
        /// </summary>
        /// <param name="deviceIdentifier"></param>
        /// <param name="ip"></param>
        /// <param name="ex"></param>
        [LoggerMessage(EventId = 1003, EventName = "Not Connected to Host", Level = LogLevel.Warning, Message = "Not connected to {deviceIdentifier} at {ip}")]
        public partial void NotConnectedToHostException(string deviceIdentifier, IPAddress ip, Exception ex = null);

        /// <summary>
        /// Disconnecting from host message
        /// </summary>
        /// <param name="deviceIdentifier"></param>
        /// <param name="ip"></param>
        [LoggerMessage(EventId = 1004, EventName = "Host Disconnecting", Level = LogLevel.Debug, Message = "Disconnecting from {deviceIdentifier} at {ip}")]
        public partial void DisconnectingFromHostMessage(string deviceIdentifier, IPAddress ip);

        /// <summary>
        /// Disconnecting from host exception message
        /// </summary>
        /// <param name="deviceIdentifier"></param>
        /// <param name="ip"></param>
        /// <param name="ex"></param>
        [LoggerMessage(EventId = 1005, EventName = "Host Disconnecting Exception", Level = LogLevel.Warning, Message = "Exception disconnecting from {deviceIdentifier} at {ip}")]
        public partial void DisconnectingFromHostException(string deviceIdentifier, IPAddress ip, Exception ex = null);

        #endregion

        #region ListResourcesMessages

        /// <summary>
        /// Get path listing message
        /// </summary>
        /// <param name="deviceIdentifier"></param>
        /// <param name="ip"></param>
        /// <param name="path"></param>
        [LoggerMessage(EventId = 1010, EventName = "Getting Resources List", Level = LogLevel.Debug, Message = "Getting resources {path} from {deviceIdentifier} at {ip}")]
        public partial void GettingsResourcesListMessage(string deviceIdentifier, IPAddress ip, string path);

        /// <summary>
        /// Path listing message
        /// </summary>
        /// <param name="total"></param>
        /// <param name="deviceIdentifier"></param>
        /// <param name="ip"></param>
        [LoggerMessage(EventId = 1011, EventName = "Resource Listing", Level = LogLevel.Information, Message = "{total} files found on {deviceIdentifier} at {ip}")]
        public partial void ResourceListingMessage(int total, string deviceIdentifier, IPAddress ip);

        /// <summary>
        /// Path listing exception message
        /// </summary>
        /// <param name="deviceIdentifier"></param>
        /// <param name="ip"></param>
        /// <param name="path"></param>
        /// <param name="ex"></param>
        [LoggerMessage(EventId = 1012, EventName = "Resources Listing Exception", Level = LogLevel.Warning, Message = "Exception getting resources {path} from {deviceIdentifier} at {ip}")]
        public partial void ResourceListingException(string deviceIdentifier, IPAddress ip, string path, Exception ex = null);

        #endregion

        #region ResourceDownloadMessages

        /// <summary>
        /// Downloading resource message
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="deviceIdentifier"></param>
        /// <param name="ip"></param>
        [LoggerMessage(EventId = 1020, EventName = "Downloading Resource", Level = LogLevel.Debug, Message = "Downloading resource {resource} from {deviceIdentifier} at {ip}")]
        public partial void DownloadingResourceMessage(Uri resource, string deviceIdentifier, IPAddress ip);

        /// <summary>
        /// Downloaded resource message
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="deviceIdentifier"></param>
        /// <param name="ip"></param>
        [LoggerMessage(EventId = 1021, EventName = "Downloaded Resource", Level = LogLevel.Debug, Message = "Downloaded resource {resource} from {deviceIdentifier} at {ip}")]
        public partial void DownloadedResourceMessage(Uri resource, string deviceIdentifier, IPAddress ip);

        /// <summary>
        /// Downloaded resources message
        /// </summary>
        /// <param name="current"></param>
        /// <param name="total"></param>
        /// <param name="deviceIdentifier"></param>
        /// <param name="ip"></param>
        [LoggerMessage(EventId = 1022, EventName = "Downloaded Resources", Level = LogLevel.Information, Message = "Downloaded {current}/{total} resources from {deviceIdentifier} at {ip}")]
        public partial void DownloadedResourcesMessage(int current, int total, string deviceIdentifier, IPAddress ip);

        /// <summary>
        /// Downloaded resource exception message
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="deviceIdentifier"></param>
        /// <param name="ip"></param>
        /// <param name="ex"></param>
        [LoggerMessage(EventId = 1023, EventName = "Download Resource Exception", Level = LogLevel.Warning, Message = "Exception downloading resource {resource} from {deviceIdentifier} at {ip}")]
        public partial void DownloadResourceException(Uri resource, string deviceIdentifier, IPAddress ip, Exception ex = null);

        #endregion

        #region DeleteResourceMessages

        /// <summary>
        /// Deleting resource message
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="deviceIdentifier"></param>
        /// <param name="ip"></param>
        [LoggerMessage(EventId = 1030, EventName = "Deleting Resource", Level = LogLevel.Debug, Message = "Deleting resource {resource} from {deviceIdentifier} at {ip}")]
        public partial void DeletingResourceMessage(Uri resource, string deviceIdentifier, IPAddress ip);

        /// <summary>
        /// Deleted resource message
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="deviceIdentifier"></param>
        /// <param name="ip"></param>
        [LoggerMessage(EventId = 1031, EventName = "Deleted Resource", Level = LogLevel.Debug, Message = "Deleted resource {resource} from {deviceIdentifier} at {ip}")]
        public partial void DeletedResourceMessage(Uri resource, string deviceIdentifier, IPAddress ip);

        /// <summary>
        /// Delete resource exception message
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="deviceIdentifier"></param>
        /// <param name="ip"></param>
        /// <param name="ex"></param>
        [LoggerMessage(EventId = 1032, EventName = "Delete Resource Exception", Level = LogLevel.Warning, Message = "Exception deleting resource {resource} from {deviceIdentifier} at {ip}")]
        public partial void DeleteResourceException(Uri resource, string deviceIdentifier, IPAddress ip, Exception ex = null);

        #endregion

        /// <summary>
        /// Invalid device ipaddress exception message
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="ex"></param>
        [LoggerMessage(EventId = 9000, EventName = "Invalid ipaddress Exception", Level = LogLevel.Warning, Message = "Exception validating or connecting to {ip}")]
        public partial void InvalidDeviceIpAddressException(IPAddress ip, Exception ex = null);

        /// <summary>
        /// Operation cancelled exception message
        /// </summary>
        /// <param name="deviceIdentifier"></param>
        /// <param name="ip"></param>
        /// <param name="ex"></param>
        [LoggerMessage(EventId = 9001, EventName = "Operation Canceled Exception", Level = LogLevel.Information, Message = "Operation canceled downloading from {deviceIdentifier} at {ip}")]
        public partial void OperationCancelledException(string deviceIdentifier, IPAddress ip, Exception ex = null);
    }
}