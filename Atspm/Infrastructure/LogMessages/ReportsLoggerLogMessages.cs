using Google.Cloud.Diagnostics.Common;
using Microsoft.Extensions.Logging;

namespace Utah.Udot.Atspm.Infrastructure.LogMessages
{
    /// <summary>
    /// Log messages for <see cref="IReportService{Tin, Tout}"/> implementations
    /// </summary>
    public partial class ReportsLoggerLogMessages<Tin, Tout>
    {
        private readonly ILogger _logger;

        //TODO: This logger has been placed in the controllers for the ReportApi when it should be injected into the IReportService directoy and log there, Controllers have their own logging mechanism

        /// <summary>
        /// Log messages for <see cref="IReportService{Tin, Tout}"/> implementations
        /// </summary>
        /// <param name="logger">Abstract logging providers</param>
        /// <param name="reportService">Reporting service to log messages for</param>
        public ReportsLoggerLogMessages(ILogger logger, IReportService<Tin, Tout> reportService)
        {
            _logger = logger.WithAddedLabels(new Dictionary<string, string>()
            {
                { "ReportService", reportService.GetType().Name }
            });
        }

        /// <summary>
        /// Report has started
        /// </summary>
        /// <param name="time"></param>
        /// <param name="name"></param>
        [LoggerMessage(EventId = 1101, EventName = "Report has started", Level = LogLevel.Information, Message = "Report has started at {time}, name: {name}")]
        public partial void ReportStartedMessage(DateTime time, string name);

        /// <summary>
        /// Report has completed
        /// </summary>
        /// <param name="time"></param>
        /// <param name="name"></param>
        [LoggerMessage(EventId = 1102, EventName = "Report has completed", Level = LogLevel.Information, Message = "Report has completed at {time}, name: {name}")]
        public partial void ReportCompletedMessage(DateTime time, string name);

        /// <summary>
        /// Report exception
        /// </summary>
        /// <param name="ex"></param>
        [LoggerMessage(EventId = 1103, EventName = "Report execution exception", Level = LogLevel.Error, Message = "Exception executing Report")]
        public partial void ReportExecutionException(Exception ex = null);
    }
}
