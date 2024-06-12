#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.LogMessages/ControllerLoggerDecoderLogMessages.cs
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