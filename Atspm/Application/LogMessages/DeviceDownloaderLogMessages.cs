#region license
// Copyright 2024 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.LogMessages/DeviceDownloaderLogMessages.cs
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
using Utah.Udot.Atspm.Services;

namespace Utah.Udot.Atspm.LogMessages
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
        /// <param name="logger">Abstract logging providers</param>
        /// <param name="device">Device to download</param>
        public DeviceDownloaderLogMessages(ILogger logger, Device device)
        {
            _logger = logger.WithAddedLabels(new Dictionary<string, string>()
            {
                { "locationIdentifier", device?.Location?.LocationIdentifier },
                { "LocationName", device?.Location?.PrimaryName },
                { "DeviceId", device.Id.ToString() },
                { "DeviceType", device?.DeviceType.ToString() },
                { "IPAddress", device?.Ipaddress.ToString() },
            });
        }

        #region HostConnectionMessages

        /// <summary>
        /// Connecting to host message
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="ip"></param>
        [LoggerMessage(EventId = 1000, EventName = "Host Connecting", Level = LogLevel.Debug, Message = "Connecting to {locationId} at {ip}")]
        public partial void ConnectingToHostMessage(string locationId, IPAddress ip);

        /// <summary>
        /// Connected to host message
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="ip"></param>
        [LoggerMessage(EventId = 1001, EventName = "Host Connected", Level = LogLevel.Debug, Message = "Connected to {locationId} at {ip}")]
        public partial void ConnectedToHostMessage(string locationId, IPAddress ip);

        /// <summary>
        /// Connecting to host exception
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="ip"></param>
        /// <param name="ex"></param>
        [LoggerMessage(EventId = 1002, EventName = "Host Connection Exception", Level = LogLevel.Warning, Message = "Exception connecting to {locationId} at {ip}")]
        public partial void ConnectingToHostException(string locationId, IPAddress ip, Exception ex = null);

        /// <summary>
        /// Not connected to host exception
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="ip"></param>
        /// <param name="ex"></param>
        [LoggerMessage(EventId = 1003, EventName = "Not Connected to Host", Level = LogLevel.Warning, Message = "Not connected to {locationId} at {ip}")]
        public partial void NotConnectedToHostException(string locationId, IPAddress ip, Exception ex = null);

        /// <summary>
        /// Disconnecting from host message
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="ip"></param>
        [LoggerMessage(EventId = 1004, EventName = "Host Disconnecting", Level = LogLevel.Debug, Message = "Disconnecting from {locationId} at {ip}")]
        public partial void DisconnectingFromHostMessage(string locationId, IPAddress ip);

        /// <summary>
        /// Disconnecting from host exception message
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="ip"></param>
        /// <param name="ex"></param>
        [LoggerMessage(EventId = 1005, EventName = "Host Disconnecting Exception", Level = LogLevel.Warning, Message = "Exception disconnecting from {locationId} at {ip}")]
        public partial void DisconnectingFromHostException(string locationId, IPAddress ip, Exception ex = null);

        #endregion

        #region ListDirectoryMessages

        /// <summary>
        /// Get directory listing message
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="ip"></param>
        /// <param name="directory"></param>
        [LoggerMessage(EventId = 1010, EventName = "Getting Directory List", Level = LogLevel.Debug, Message = "Getting directory {directory} from {locationId} at {ip}")]
        public partial void GettingDirectoryListMessage(string locationId, IPAddress ip, string directory);

        /// <summary>
        /// Directory listing message
        /// </summary>
        /// <param name="total"></param>
        /// <param name="locationId"></param>
        /// <param name="ip"></param>
        [LoggerMessage(EventId = 1011, EventName = "Directory Listing", Level = LogLevel.Information, Message = "{total} files found on {locationId} at {ip}")]
        public partial void DirectoryListingMessage(int total, string locationId, IPAddress ip);

        /// <summary>
        /// Directory listing exception message
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="ip"></param>
        /// <param name="directory"></param>
        /// <param name="ex"></param>
        [LoggerMessage(EventId = 1012, EventName = "Directory Listing Exception", Level = LogLevel.Warning, Message = "Exception getting directory {directory} from {locationId} at {ip}")]
        public partial void DirectoryListingException(string locationId, IPAddress ip, string directory, Exception ex = null);

        #endregion

        #region FileDownloadMessages

        /// <summary>
        /// Downloading file message
        /// </summary>
        /// <param name="file"></param>
        /// <param name="locationId"></param>
        /// <param name="ip"></param>
        [LoggerMessage(EventId = 1020, EventName = "Downloading File", Level = LogLevel.Debug, Message = "Downloading file {file} from {locationId} at {ip}")]
        public partial void DownloadingFileMessage(string file, string locationId, IPAddress ip);

        /// <summary>
        /// Downloaded file message
        /// </summary>
        /// <param name="file"></param>
        /// <param name="locationId"></param>
        /// <param name="ip"></param>
        [LoggerMessage(EventId = 1021, EventName = "Downloaded File", Level = LogLevel.Debug, Message = "Downloaded file {file} from {locationId} at {ip}")]
        public partial void DownloadedFileMessage(string file, string locationId, IPAddress ip);

        /// <summary>
        /// Downloaded files message
        /// </summary>
        /// <param name="current"></param>
        /// <param name="total"></param>
        /// <param name="locationId"></param>
        /// <param name="ip"></param>
        [LoggerMessage(EventId = 1022, EventName = "Downloaded Files", Level = LogLevel.Information, Message = "Downloaded {current}/{total} files from {locationId} at {ip}")]
        public partial void DownloadedFilesMessage(int current, int total, string locationId, IPAddress ip);

        /// <summary>
        /// Downloaded file exception message
        /// </summary>
        /// <param name="file"></param>
        /// <param name="locationId"></param>
        /// <param name="ip"></param>
        /// <param name="ex"></param>
        [LoggerMessage(EventId = 1023, EventName = "Download File Exception", Level = LogLevel.Warning, Message = "Exception downloading file {file} from {locationId} at {ip}")]
        public partial void DownloadFileException(string file, string locationId, IPAddress ip, Exception ex = null);

        #endregion

        #region FileDeleteMessages

        /// <summary>
        /// Deleting file message
        /// </summary>
        /// <param name="file"></param>
        /// <param name="locationId"></param>
        /// <param name="ip"></param>
        [LoggerMessage(EventId = 1030, EventName = "Deleting File", Level = LogLevel.Debug, Message = "Deleting file {file} from {locationId} at {ip}")]
        public partial void DeletingFileMessage(string file, string locationId, IPAddress ip);

        /// <summary>
        /// Deleted file message
        /// </summary>
        /// <param name="file"></param>
        /// <param name="locationId"></param>
        /// <param name="ip"></param>
        [LoggerMessage(EventId = 1031, EventName = "Deleted File", Level = LogLevel.Debug, Message = "Deleted file {file} from {locationId} at {ip}")]
        public partial void DeletedFileMessage(string file, string locationId, IPAddress ip);

        /// <summary>
        /// Delete file exception message
        /// </summary>
        /// <param name="file"></param>
        /// <param name="locationId"></param>
        /// <param name="ip"></param>
        /// <param name="ex"></param>
        [LoggerMessage(EventId = 1032, EventName = "Delete File Exception", Level = LogLevel.Warning, Message = "Exception deleting file {file} from {locationId} at {ip}")]
        public partial void DeleteFileException(string file, string locationId, IPAddress ip, Exception ex = null);

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
        /// <param name="locationId"></param>
        /// <param name="ip"></param>
        /// <param name="ex"></param>
        [LoggerMessage(EventId = 9001, EventName = "Operation Canceled Exception", Level = LogLevel.Information, Message = "Operation canceled downloading from {locationId} at {ip}")]
        public partial void OperationCancelledException(string locationId, IPAddress ip, Exception ex = null);
    }
}
