using ATSPM.Application.Business;
using ATSPM.Application.Business.PreempDetail;
using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Application.TempExtensions;
using ATSPM.Data.Enums;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Preempt detail report service
    /// </summary>
    public class PreemptDetailReportService : ReportServiceBase<PreemptDetailOptions, PreemptDetailResult>
    {
        private readonly IIndianaEventLogRepository controllerEventLogRepository;
        private readonly PreemptDetailService preemptDetailService;
        private readonly ILocationRepository LocationRepository;

        /// <inheritdoc/>
        public PreemptDetailReportService(
            IIndianaEventLogRepository controllerEventLogRepository,
            PreemptDetailService preemptDetailService,
            ILocationRepository LocationRepository)
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.preemptDetailService = preemptDetailService;
            this.LocationRepository = LocationRepository;
        }

        /// <inheritdoc/>
        public override async Task<PreemptDetailResult> ExecuteAsync(PreemptDetailOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var Location = LocationRepository.GetLatestVersionOfLocation(parameter.LocationIdentifier, parameter.Start);
            if (Location == null)
            {
                //return BadRequest("Location not found");
                return await Task.FromException<PreemptDetailResult>(new NullReferenceException("Location not found"));
            }
            var codes = new List<DataLoggerEnum>();

            for (var i = 101; i <= 111; i++)
                codes.Add((DataLoggerEnum)i);

            var events = controllerEventLogRepository.GetLocationEventsByEventCodes(
                parameter.LocationIdentifier,
                parameter.Start,
                parameter.End,
                codes).ToList();

            if (events.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for Location");
                return await Task.FromException<PreemptDetailResult>(new NullReferenceException("No Controller Event Logs found for Location"));
            }


            var result = preemptDetailService.GetChartData(parameter, events);
            //viewModel.Details = Location.LocationDescription();
            result.Summary.LocationDescription = Location.LocationDescription();
            //return Ok(viewModel);
            foreach (var item in result.Details)
            {
                item.LocationDescription = Location.LocationDescription();
            }
            return result;
        }
    }
}
