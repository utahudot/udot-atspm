#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.LogMessages/LocationControllerLoggerLogMessages.cs
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