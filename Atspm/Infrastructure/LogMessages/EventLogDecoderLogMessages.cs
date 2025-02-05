using Google.Cloud.Diagnostics.Common;
using Microsoft.Extensions.Logging;

namespace Utah.Udot.Atspm.Infrastructure.LogMessages
{
    /// <summary>
    /// Log messages for <see cref="IEventLogDecoder{T}"/> implementations
    /// </summary>
    public partial class EventLogDecoderLogMessages
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Log messages for <see cref="IEventLogDecoder{T}"/> implementations
        /// </summary>
        /// <param name="logger">Abstract logging providers</param>
        /// <param name="file">File to decode</param>
        public EventLogDecoderLogMessages(ILogger logger, FileInfo file)
        {
            _logger = logger.WithAddedLabels(new Dictionary<string, string>()
            {
                { "FileName", file.FullName }
            });
        }

        #region DecodeFile

        /// <summary>
        /// Logs <paramref name="file"/> that is going to be decoded
        /// </summary>
        /// <param name="file">File to decode</param>
        [LoggerMessage(EventId = 1000, EventName = "Decoding Log File", Level = LogLevel.Debug, Message = "Decoding {file}")]
        public partial void DecodeLogFileMessage(string file);

        /// <summary>
        /// Logs the count of events from <paramref name="file"/>
        /// </summary>
        /// <param name="file">File to decode</param>
        /// <param name="count">Count of events decoded from <paramref name="file"/></param>
        [LoggerMessage(EventId = 1001, EventName = "Decoded Log File", Level = LogLevel.Information, Message = "Decoded {count} events from {file}")]
        public partial void DecodedLogsMessage(string file, int count);

        /// <summary>
        /// Logs exceptions that occured while decoding <paramref name="file"/>
        /// </summary>
        /// <param name="file">File to decode</param>
        /// <param name="ex">Exception thrown decoding <paramref name="file"/></param>
        [LoggerMessage(EventId = 1002, EventName = "Decode Exception", Level = LogLevel.Warning, Message = "Exception decoding {file}")]
        public partial void DecodeLogFileException(string file, Exception ex);

        #endregion

        /// <summary>
        /// Operation cancelled exception message
        /// </summary>
        /// <param name="file">File to decode</param>
        /// <param name="ex"></param>
        [LoggerMessage(EventId = 9001, EventName = "Operation Canceled Exception", Level = LogLevel.Information, Message = "Operation canceled decoding file {file}")]
        public partial void OperationCancelledException(string file, Exception ex = null);
    }
}
