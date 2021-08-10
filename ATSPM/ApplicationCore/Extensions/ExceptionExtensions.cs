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
