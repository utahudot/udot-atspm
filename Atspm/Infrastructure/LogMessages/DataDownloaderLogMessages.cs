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
using Microsoft.Extensions.Logging;

namespace Utah.Udot.Atspm.Infrastructure.LogMessages
{
    public partial class DataDownloaderLogMessages
    {
        private readonly ILogger _logger;

        public DataDownloaderLogMessages(ILogger logger, DataDownloadLog dataDownloadLog)
        {
            _logger = logger.WithAddedLabels(new Dictionary<string, string?>
            {
                { nameof(DataDownloadLog.TraceId), dataDownloadLog.TraceId },
                { nameof(DataDownloadLog.UserId), dataDownloadLog.UserId },
                { nameof(DataDownloadLog.Route), dataDownloadLog.Route },
                { nameof(DataDownloadLog.Method), dataDownloadLog.Method },
                { nameof(DataDownloadLog.StatusCode), dataDownloadLog.StatusCode.ToString() },
                { nameof(DataDownloadLog.DurationMs), dataDownloadLog.DurationMs.ToString() },
                { nameof(DataDownloadLog.Controller), dataDownloadLog.Controller },
                { nameof(DataDownloadLog.Action), dataDownloadLog.Action },
                { nameof(DataDownloadLog.RemoteIp), dataDownloadLog.RemoteIp },
                { nameof(DataDownloadLog.UserAgent), dataDownloadLog.UserAgent }
            });
        }

        [LoggerMessage(EventId = 200, EventName = "Data Download Successful", Level = LogLevel.Information, Message = "Completed request: {dataDownloadLog}")]
        public partial void DataDownloadSuccessful(DataDownloadLog dataDownloadLog);
    }
}
