using Microsoft.IdentityModel.Tokens;
using Utah.Udot.Atspm.Business.RampMetering;

namespace Utah.Udot.ATSPM.ReportApi.ReportServices
{
    public class RampMeteringReportService : ReportServiceBase<RampMeteringOptions, RampMeteringResult>
    {
        private readonly IIndianaEventLogRepository controllerEventLogRepository;
        private readonly ILocationRepository LocationRepository;

        public RampMeteringReportService(IIndianaEventLogRepository controllerEventLogRepository, ILocationRepository locationRepository)
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
            LocationRepository = locationRepository;
        }

        public override async Task<RampMeteringResult> ExecuteAsync(RampMeteringOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var location = LocationRepository.GetLatestVersionOfLocation(parameter.LocationIdentifier, parameter.Start);
            if (location == null)
            {
                //return BadRequest("Location not found");
                return await Task.FromException<RampMeteringResult>(new NullReferenceException("Location not found"));
            }
            var controllerEventLogs = controllerEventLogRepository.GetEventsBetweenDates(location.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();
            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for Location");
                return await Task.FromException<RampMeteringResult>(new NullReferenceException("No Controller Event Logs found for Location"));
            }

            var mainlineAvgSpeedEventsCodes = new List<int>() { 1371 };
            var mainlineAvgOccurrenceEventsCodes = new List<int>() { 1372 };
            var mainlineAvgFlowEventsCodes = new List<int>() { 1373 };

            //codes for active rate 1058-1073
            //codes for base rate 1042-1057
            // codes for start up warning 1004
            // codes for shut down warning 1014
            // codes for L1 Queue On 1171
            // codes for L2 Queue On 1173
            // codes for L3 Queue On 1175
            // codes for L1 Queue Off 1170
            // codes for L2 Queue Off 1172
            // codes for L3 Queue Off 1174

            var mainlineAvgSpeedEvents = controllerEventLogs.Where(e => mainlineAvgSpeedEventsCodes.Contains(e.EventCode));
            var mainlineAvgOccurrenceEvents = controllerEventLogs.Where(e => mainlineAvgOccurrenceEventsCodes.Contains(e.EventCode));
            var mainlineAvgFlowEvents = controllerEventLogs.Where(e => mainlineAvgFlowEventsCodes.Contains(e.EventCode));

            var mainlineAvgSpeeds = mainlineAvgSpeedEvents.Select(e => new DataPointDateDouble(e.Timestamp, e.EventParam)).ToList();
            var mainlineAvgOccurrence = mainlineAvgOccurrenceEvents.Select(e => new DataPointDateDouble(e.Timestamp, e.EventParam)).ToList();
            var mainlineAvgFlow = mainlineAvgFlowEvents.Select(e => new DataPointDateDouble(e.Timestamp, e.EventParam)).ToList();

            var planEvents = controllerEventLogs.GetPlanEvents(
                parameter.Start.AddHours(-12),
                parameter.End.AddHours(12)).ToList();

            return new RampMeteringResult(location.LocationIdentifier, parameter.Start, parameter.End)
            {
                MainLineAvgFlow = mainlineAvgFlow,
                MainlineAvgOcc = mainlineAvgOccurrence,
                MainlineAvgSpeed = mainlineAvgSpeeds,
            };
        }
    }
}
