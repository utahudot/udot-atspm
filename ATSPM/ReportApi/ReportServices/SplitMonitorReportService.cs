using ATSPM.Application.Repositories;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.SplitMonitor;
using ATSPM.ReportApi.TempExtensions;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Split monitor report service
    /// </summary>
    public class SplitMonitorReportService : ReportServiceBase<SplitMonitorOptions, IEnumerable<SplitMonitorResult>>
    {
        private readonly SplitMonitorService splitMonitorService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ILocationRepository LocationRepository;

        /// <inheritdoc/>
        public SplitMonitorReportService(
            SplitMonitorService splitMonitorService,
            IControllerEventLogRepository controllerEventLogRepository,
            ILocationRepository LocationRepository)
        {
            this.splitMonitorService = splitMonitorService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.LocationRepository = LocationRepository;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<SplitMonitorResult>> ExecuteAsync(SplitMonitorOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var Location = LocationRepository.GetLatestVersionOfLocation(parameter.locationIdentifier, parameter.Start);

            if (Location == null)
            {
                //return BadRequest("Location not found");
                return await Task.FromException<IEnumerable<SplitMonitorResult>>(new NullReferenceException("Location not found"));
            }

            var controllerEventLogs = controllerEventLogRepository.GetLocationEventsBetweenDates(Location.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();

            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for Location");
                return await Task.FromException<IEnumerable<SplitMonitorResult>>(new NullReferenceException("No Controller Event Logs found for Location"));
            }

            var planEvents = controllerEventLogs.GetPlanEvents(
            parameter.Start.AddHours(-12),
                parameter.End.AddHours(12)).ToList();
            var pedEvents = controllerEventLogs.Where(e =>
                new List<int> { 21, 23 }.Contains(e.EventCode)
                && e.Timestamp >= parameter.Start
                && e.Timestamp <= parameter.End).ToList();
            var cycleEvents = controllerEventLogs.Where(e =>
                new List<int> { 1, 4, 5, 6, 7, 8, 11 }.Contains(e.EventCode)
                && e.Timestamp >= parameter.Start
                && e.Timestamp <= parameter.End).ToList();
            var splitsEventCodes = new List<int>();
            for (var i = 130; i <= 151; i++)
                splitsEventCodes.Add(i);
            var splitsEvents = controllerEventLogs.Where(e =>
                splitsEventCodes.Contains(e.EventCode)
                && e.Timestamp >= parameter.Start
                && e.Timestamp <= parameter.End).ToList();
            var terminationEvents = controllerEventLogs.Where(e =>
                new List<int> { 4, 5, 6, 7 }.Contains(e.EventCode)
                && e.Timestamp >= parameter.Start
                && e.Timestamp <= parameter.End).ToList();


            controllerEventLogs = null;

            var results = await splitMonitorService.GetChartData(
               parameter,
               planEvents,
               cycleEvents,
               pedEvents,
               splitsEvents,
               terminationEvents,
               Location);

            var finalResultcheck = results.Where(result => result != null).ToList();

            //if (finalResultcheck.IsNullOrEmpty())
            //{
            //    return Ok("No chart data found");
            //}

            //return Ok(finalResultcheck);
            return finalResultcheck;
        }
    }
}
