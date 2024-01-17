using Microsoft.Extensions.Logging;

namespace ATSPM.Application.Events
{
    /// <summary>
    /// <see cref="EventId"/> for atspm application logging
    /// </summary>
    public static class EventCodes
    {
        #region DeviceDownloader

        /// <summary>
        /// Connecting to host
        /// </summary>
        public static EventId DeviceDownloaderHostConnecting { get; } = new(1101, "DeviceDownloaderHostConnecting");

        /// <summary>
        /// Host is connected
        /// </summary>
        public static EventId DeviceDownloaderHostConnected { get; } = new(1102, "DeviceDownloaderHostConnected");

        /// <summary>
        /// Exception connecting to host
        /// </summary>
        public static EventId DeviceDownloaderHostConnectionException { get; } = new(1103, "DeviceDownloaderHostConnectionException");

        /// <summary>
        /// Not connected to host
        /// </summary>
        public static EventId DeviceDownloaderNotConnectedtoHost { get; } = new(1104, "DeviceDownloaderNotConnectedtoHost");

        /// <summary>
        /// Host disconnecting
        /// </summary>
        public static EventId DeviceDownloaderHostDisconnecting { get; } = new(1105, "DeviceDownloaderHostDisconnecting");

        /// <summary>
        /// Exception disconnecting from host
        /// </summary>
        public static EventId DeviceDownloaderHostDisconnectingException { get; } = new(1106, "DeviceDownloaderHostDisconnectingException");

        /// <summary>
        /// Getting directory list
        /// </summary>
        public static EventId DeviceDownloaderGettingDirectoryList { get; } = new(1111, "DeviceDownloaderGettingDirectoryList");

        /// <summary>
        /// Display directory list
        /// </summary>
        public static EventId DeviceDownloaderDirectoryListing { get; } = new(1112, "DeviceDownloaderDirectoryListing");

        /// <summary>
        /// Exception with directory list
        /// </summary>
        public static EventId DeviceDownloaderDirectoryListingException { get; } = new(1113, "DeviceDownloaderDirectoryListingException");

        /// <summary>
        /// Downloading file
        /// </summary>
        public static EventId DeviceDownloaderDownloadingFile { get; } = new(1121, "DeviceDownloaderDownloadingFile");

        /// <summary>
        /// Downloaded file
        /// </summary>
        public static EventId DeviceDownloaderDownloadedFile { get; } = new(1122, "DeviceDownloaderDownloadedFile");

        /// <summary>
        /// Display downloaded files
        /// </summary>
        public static EventId DeviceDownloaderDownloadedFiles { get; } = new(1123, "DeviceDownloaderDownloadedFiles");

        /// <summary>
        /// Exception downloading files
        /// </summary>
        public static EventId DeviceDownloaderDownloadFileException { get; } = new(1124, "DeviceDownloaderDownloadFileException");

        #endregion

        #region DeviceDecoder

        /// <summary>
        /// Decoding log file
        /// </summary>
        public static EventId DeviceDecoderDecodingLogFile { get; } = new(1201, "DeviceDecoderDecodingLogFile");

        /// <summary>
        /// Display decoded log file
        /// </summary>
        public static EventId DeviceDecoderDecodedLogFile { get; } = new(1202, "DeviceDecoderDecodedLogFile");

        /// <summary>
        /// Exception decoding log file
        /// </summary>
        public static EventId DeviceDecoderDecodeLogFileException { get; } = new(1203, "DeviceDecoderDecodeLogFileException");

        #endregion

        #region Workflows

        /// <summary>
        /// Exception with operation
        /// </summary>
        public static EventId WorkflowsOperationCancelledException { get; } = new(9001, "WorkflowsOperationCancelledException");

        #endregion
    }
}
