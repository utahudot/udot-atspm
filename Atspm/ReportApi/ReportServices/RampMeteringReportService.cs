using Utah.Udot.Atspm.Business.RampMetering;

namespace Utah.Udot.ATSPM.ReportApi.ReportServices
{
    public class RampMeteringReportService : ReportServiceBase<RampMeteringOptions, RampMeteringResult>
    {
        private readonly IIndianaEventLogRepository controllerEventLogRepository;
        private readonly ILocationRepository locationRepository;
        private readonly RampMeteringService rampMeteringService;

        public RampMeteringReportService(IIndianaEventLogRepository controllerEventLogRepository, ILocationRepository locationRepository, RampMeteringService rampMeteringService)
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.locationRepository = locationRepository;
            this.rampMeteringService = rampMeteringService;
        }

        public override async Task<RampMeteringResult> ExecuteAsync(RampMeteringOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var location = locationRepository.GetLatestVersionOfLocation(parameter.LocationIdentifier, parameter.Start);
            if (location == null)
            {
                return await Task.FromException<RampMeteringResult>(new NullReferenceException("Location not found"));
            }
            var controllerEventLogs = controllerEventLogRepository.GetEventsBetweenDates(location.LocationIdentifier, parameter.Start.AddHours(-1), parameter.End.AddHours(1)).ToList();
            if (controllerEventLogs.IsNullOrEmpty())
            {
                return await Task.FromException<RampMeteringResult>(new NullReferenceException("No Controller Event Logs found for Location"));
            }

            var result = rampMeteringService.GetChartData(location, parameter, controllerEventLogs);
            return result;
        }
    }
}
