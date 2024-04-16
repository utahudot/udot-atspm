using ATSPM.Application.Business;
using ATSPM.Application.Business.PreemptService;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Application.TempExtensions;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Approach delay report service
    /// </summary>
    public class PreemptServiceReportService : ReportServiceBase<PreemptServiceOptions, PreemptServiceResult>
    {
        private readonly PreemptServiceService preemptServiceService;
        private readonly IIndianaEventLogRepository controllerEventLogRepository;
        private readonly ILocationRepository LocationRepository;

        /// <inheritdoc/>
        public PreemptServiceReportService(
            PreemptServiceService preemptServiceService,
            IIndianaEventLogRepository controllerEventLogRepository,
            ILocationRepository LocationRepository
            )
        {
            this.preemptServiceService = preemptServiceService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.LocationRepository = LocationRepository;
        }

        /// <inheritdoc/>
        public override async Task<PreemptServiceResult> ExecuteAsync(PreemptServiceOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var Location = LocationRepository.GetLatestVersionOfLocation(parameter.LocationIdentifier, parameter.Start);
            if (Location == null)
            {
                //return BadRequest("Location not found");
                return await Task.FromException<PreemptServiceResult>(new NullReferenceException("Location not found"));
            }
            var controllerEventLogs = controllerEventLogRepository.GetEventsBetweenDates(parameter.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();
            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for Location");
                return await Task.FromException<PreemptServiceResult>(new NullReferenceException("No Controller Event Logs found for Location"));
            }
            var planEvents = controllerEventLogs.GetPlanEvents(
            parameter.Start.AddHours(-12),
                parameter.End.AddHours(12)).ToList();
            var preemptEvents = controllerEventLogs.GetEventsByEventCodes(parameter.Start, parameter.End, new List<short>() { 105 });
            PreemptServiceResult result = preemptServiceService.GetChartData(
                parameter,
                planEvents.ToList(),
                preemptEvents.ToList());
            result.LocationDescription = Location.LocationDescription();
            //return Ok(viewModel);

            return result;
        }

    }
}
