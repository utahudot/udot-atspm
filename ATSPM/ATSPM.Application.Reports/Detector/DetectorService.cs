using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Approach;
using ATSPM.Application.Reports.ViewModels.YellowRedActivations;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using Google.Type;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Detector
{
    public class DetectorService
    {
        private readonly IControllerEventLogRepository _controllerEventLogRepository;
        private readonly ApproachService _approachService;

        public DetectorService(IControllerEventLogRepository controllerEventLogRepository, ApproachService approachService)
        {
            _controllerEventLogRepository = controllerEventLogRepository;
            _approachService = approachService;
        }

        public List<ControllerEventLog> GetDetectorEvents(
            List<ATSPM.Data.Models.Detector> detectors,
            string signalId,
            DateTime start,
            DateTime end)
        {
            var detectorEvents = new List<ControllerEventLog>();
            foreach (var d in detectors)
                DetectorEvents.AddRange(
                    _controllerEventLogRepository.GetEventsByEventCodesParam(
                        signalId,
                        start,
                        end,
                        new List<int> { 82 },
                        d,
                        d.GetOffset(),
                        d.LatencyCorrection));

        }

        internal void GetDetectorEvents(IEnumerable<string> enumerable, System.DateTime startDate, System.DateTime endDate)
        {
            throw new System.NotImplementedException();
        }
    }
}
