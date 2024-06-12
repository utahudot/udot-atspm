#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Extensions/ExceptionExtensions.cs
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
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;

namespace ATSPM.Application.Extensions
{
    public static class ExceptionExtensions
    {
        public static bool LogE(this Exception e, LogLevel level = LogLevel.Warning, [CallerMemberName] string caller = "")
        {
            //create logger from LogFactory so it has the custom category
            ILogger log = LoggerFactory.Create(c => c.AddConsole()).CreateLogger("Exceptions");

            //log based on log level
            log.Log(level, (int)level, e, "Caller:{Caller} - Message:{Message}", caller, e.Message);

            switch (level)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Information:
                case LogLevel.Warning:
                case LogLevel.None:
                default:
                    return false;
                case LogLevel.Error:
                case LogLevel.Critical:
                    return true; ;
            }
        }
    }
}
