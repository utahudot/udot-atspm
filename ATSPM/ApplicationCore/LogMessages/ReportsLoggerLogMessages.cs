using Microsoft.Extensions.Logging;
using System;

namespace ATSPM.Application.LogMessages
{
    public partial class ReportsLoggerLogMessages
    {
        private readonly ILogger _logger;

        public ReportsLoggerLogMessages(ILogger logger)
        {
            _logger = logger;
        }

        [LoggerMessage(EventId = 1101, EventName = "Report has started", Level = LogLevel.Information, Message = "Report has started at {time}, name: {name}")]
        public partial void ReportStartedMessage(DateTime time, string name);

        [LoggerMessage(EventId = 1102, EventName = "Report has completed", Level = LogLevel.Information, Message = "Report has completed at {time}, name: {name}")]
        public partial void ReportCompletedMessage(DateTime time, string name);

        [LoggerMessage(EventId = 1103, EventName = "Report execution exception", Level = LogLevel.Error, Message = "Exception executing Report")]
        public partial void ReportExecutionException(Exception ex = null);
    }
}
