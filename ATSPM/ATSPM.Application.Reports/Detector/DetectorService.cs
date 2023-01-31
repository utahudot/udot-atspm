using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.ViewModels.YellowRedActivations;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Detector
{
    public class DetectorService
    {
        private readonly IControllerEventLogRepository _controllerEventLogRepository;

        public DetectorService(IControllerEventLogRepository controllerEventLogRepository)
        {
            _controllerEventLogRepository = controllerEventLogRepository;
        }

        public List<ControllerEventLog> GetDetectorEvents(
            List<ATSPM.Data.Models.Detector> detectors,
            string signalId,
            DateTime start,
            DateTime end)
        {
            var detectorEvents = new List<ControllerEventLog>();
            foreach (var d in detectors)
                detectorEvents.AddRange(
                    _controllerEventLogRepository.GetEventsByEventCodesParam(
                        signalId,
                        start,
                        end,
                        new List<int> { 82 },
                        d.Id,
                        d.GetOffset(),
                        d.LatencyCorrection));
            return detectorEvents;
        }

        internal void GetDetectorEvents(IEnumerable<string> enumerable, System.DateTime startDate, System.DateTime endDate)
        {
            throw new System.NotImplementedException();
        }
    }
}
