using ATSPM.Data.Models;
using Google.Cloud.Diagnostics.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.LogMessages
{
    public partial class LocationControllerLoggerLogMessages
    {
        private readonly ILogger _logger;

        public LocationControllerLoggerLogMessages(ILogger logger)
        {
            //_logger = logger.WithAddedLabels(new Dictionary<string, string>()
            //{
            //    { "Block", file.FullName }
            //});
            _logger = logger;
        }

        [LoggerMessage(EventId = 1001, EventName = "Logger has started", Level = LogLevel.Information, Message = "Logger has started at {time} Location count: {count}")]
        public partial void LoggerStartedMessage(DateTime time, int count);

        [LoggerMessage(EventId = 1002, EventName = "Logger has completed", Level = LogLevel.Information, Message = "Logger has completed at {time} duration: {duration}")]
        public partial void LoggerCompletedMessage(DateTime time, TimeSpan duration);

        [LoggerMessage(EventId = 1003, EventName = "Logger execution exception", Level = LogLevel.Error, Message = "Exception executing logger")]
        public partial void LoggerExecutionException(Exception ex = null);

        [LoggerMessage(EventId = 2003, EventName = "Logger step has completed", Level = LogLevel.Information, Message = "{step} has completed: {status}")]
        public partial void StepCompletedMessage(string step, TaskStatus status);

        [LoggerMessage(EventId = 1003, EventName = "Logger step execution exception", Level = LogLevel.Warning, Message = "Exception executing logger step {step}")]
        public partial void StepExecutionException(string step, Exception ex = null);

    }
}