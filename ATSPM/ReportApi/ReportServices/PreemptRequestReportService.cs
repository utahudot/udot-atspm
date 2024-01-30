using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.PreemptServiceRequest;
using ATSPM.ReportApi.TempExtensions;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Preempt request report service
    /// </summary>
    public class PreemptRequestReportService : ReportServiceBase<PreemptServiceRequestOptions, PreemptServiceRequestResult>
    {
        private readonly PreemptServiceRequestService preemptServiceRequestService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ILocationRepository LocationRepository;

        /// <inheritdoc/>
        public PreemptRequestReportService(
            PreemptServiceRequestService preemptServiceRequestService,
            IControllerEventLogRepository controllerEventLogRepository,
            ILocationRepository LocationRepository)
        {
            this.preemptServiceRequestService = preemptServiceRequestService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.LocationRepository = LocationRepository;
        }

        /// <inheritdoc/>
        public override async Task<PreemptServiceRequestResult> ExecuteAsync(PreemptServiceRequestOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var Location = LocationRepository.GetLatestVersionOfLocation(parameter.locationIdentifier, parameter.Start);
            if (Location == null)
            {
                //return BadRequest("Location not found");
                return await Task.FromException<PreemptServiceRequestResult>(new NullReferenceException("Location not found"));
            }
            var controllerEventLogs = controllerEventLogRepository.GetLocationEventsBetweenDates(parameter.locationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();
            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for Location");
                return await Task.FromException<PreemptServiceRequestResult>(new NullReferenceException("No Controller Event Logs found for Location"));
            }
            var planEvents = controllerEventLogs.GetPlanEvents(
            parameter.Start.AddHours(-12),
                parameter.End.AddHours(12)).ToList();
            var events = controllerEventLogs.GetEventsByEventCodes(parameter.Start, parameter.End, new List<int>() { 102 });
            PreemptServiceRequestResult result = preemptServiceRequestService.GetChartData(parameter, planEvents, events);
            result.LocationDescription = Location.LocationDescription();
            //return Ok(viewModel);

            return result;
        }
    }
}
