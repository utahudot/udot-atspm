using ATSPM.Data.Models;
using Google.Cloud.Diagnostics.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Application.LogMessages
{
    public partial class ControllerLoggerDownloaderLogMessages
    {
        private readonly ILogger _logger;

        public ControllerLoggerDownloaderLogMessages(ILogger logger, Signal signal)
        {
            _logger = logger.WithAddedLabels(new Dictionary<string, string>()
            {
                { "SignalID", signal?.SignalID },
                { "SignalName", signal?.PrimaryName },
                { "SignalTypeID", signal?.ControllerType?.ControllerTypeID.ToString() },
                { "IPAddress", signal?.IPAddress },
            });
        }

        #region HostConnectionMessages

        [LoggerMessage(EventId = 1000, EventName = "Host Connecting", Level = LogLevel.Debug, Message = "Connecting to {SignalID} at {ip}")]
        public partial void ConnectingToHostMessage(string SignalID, string ip);

        [LoggerMessage(EventId = 1001, EventName = "Host Connected", Level = LogLevel.Debug, Message = "Connected to {SignalID} at {ip}")]
        public partial void ConnectedToHostMessage(string SignalID, string ip);

        [LoggerMessage(EventId = 1002, EventName = "Host Connection Exception", Level = LogLevel.Warning, Message = "Exception connecting to {SignalID} at {ip}")]
        public partial void ConnectingToHosException(string SignalID, string ip, Exception ex = null);

        [LoggerMessage(EventId = 1003, EventName = "Not Connected to Host", Level = LogLevel.Warning, Message = "Not connected to {SignalID} at {ip}")]
        public partial void NotConnectedToHostException(string SignalID, string ip, Exception ex = null);

        [LoggerMessage(EventId = 1004, EventName = "Host Disconnecting", Level = LogLevel.Debug, Message = "Disconnecting from {SignalID} at {ip}")]
        public partial void DisconnectingFromHostMessage(string SignalID, string ip);

        [LoggerMessage(EventId = 1005, EventName = "Host Disconnecting Exception", Level = LogLevel.Warning, Message = "Exception disconnecting from {SignalID} at {ip}")]
        public partial void DisconnectingFromHostException(string SignalID, string ip, Exception ex = null);

        #endregion

        #region ListDirectoryMessages

        [LoggerMessage(EventId = 1010, EventName = "Getting Directory List", Level = LogLevel.Debug, Message = "Getting directory from {SignalID} at {ip}")]
        public partial void GettingDirectoryListMessage(string SignalID, string ip);

        [LoggerMessage(EventId = 1011, EventName = "Directory Listing", Level = LogLevel.Debug, Message = "{total} files found on {SignalID} at {ip}")]
        public partial void DirectoryListingMessage(int total, string SignalID, string ip);

        [LoggerMessage(EventId = 1012, EventName = "Directory Listing Exception", Level = LogLevel.Warning, Message = "Exception getting directory from {SignalID} at {ip}")]
        public partial void DirectoryListingException(string SignalID, string ip, Exception ex = null);

        #endregion

        #region FileDownloadMessages

        [LoggerMessage(EventId = 1020, EventName = "Downloading File", Level = LogLevel.Debug, Message = "Downloading file {file} from {SignalID} at {ip}")]
        public partial void DownloadingFileMessage(string file, string SignalID, string ip);

        [LoggerMessage(EventId = 1021, EventName = "Downloaded File", Level = LogLevel.Debug, Message = "Downloaded file {file} from {SignalID} at {ip}")]
        public partial void DownloadedFileMessage(string file, string SignalID, string ip);

        [LoggerMessage(EventId = 1022, EventName = "Downloaded Files", Level = LogLevel.Information, Message = "Downloaded {current}/{total} files from {SignalID} at {ip}")]
        public partial void DownloadedFilesMessage(int current, int total, string SignalID, string ip);

        [LoggerMessage(EventId = 1023, EventName = "Download File Exception", Level = LogLevel.Warning, Message = "Exception downloading file {file} from {SignalID} at {ip}")]
        public partial void DownloadFileException(string file, string SignalID, string ip, Exception ex = null);

        #endregion

        [LoggerMessage(EventId = 9001, EventName = "Operation Canceled Exception", Level = LogLevel.Information, Message = "Operation canceled connecting to {SignalID} at {ip}")]
        public partial void OperationCancelledException(string SignalID, string ip, Exception ex = null);
    }
}
