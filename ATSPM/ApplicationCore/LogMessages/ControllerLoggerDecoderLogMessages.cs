using ATSPM.Data.Models;
using Google.Cloud.Diagnostics.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ATSPM.Application.LogMessages
{
    public partial class ControllerLoggerDecoderLogMessages
    {
        private readonly ILogger _logger;

        public ControllerLoggerDecoderLogMessages(ILogger logger, FileInfo file)
        {
            _logger = logger.WithAddedLabels(new Dictionary<string, string>()
            {
                { "FileName", file.FullName }
            });
        }

        #region DecodeFile

        [LoggerMessage(EventId = 1000, EventName = "Decoding Log File", Level = LogLevel.Debug, Message = "Decoding {file}")]
        public partial void DecodeLogFileMessage(string file);

        [LoggerMessage(EventId = 1001, EventName = "Decoded Log File", Level = LogLevel.Information, Message = "Decoded {count} logs from {file}")]
        public partial void DecodedLogsMessage(string file, int count);

        [LoggerMessage(EventId = 1002, EventName = "Decode Exception", Level = LogLevel.Warning, Message = "Exception decoding {file}")]
        public partial void DecodeLogFileException(string file, Exception ex);

        #endregion
    }
}