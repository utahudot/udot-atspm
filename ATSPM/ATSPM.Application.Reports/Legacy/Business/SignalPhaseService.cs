using ATSPM.Application.Repositories;
using ATSPM.Application.Extensions;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Legacy.Common.Business
{
    public class SignalPhaseService
    {
        private readonly bool _showVolume;
        private readonly int _binSize;
        private readonly int _metricTypeId;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly PlanService planService;
        private readonly CycleService cycleService;
        private readonly int _pcdCycleTime = 0;

        public SignalPhaseService(
            IControllerEventLogRepository controllerEventLogRepository,
            PlanService planService,
            CycleService cycleService
            )
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.planService = planService;
            this.cycleService = cycleService;
        }

       

        public void LinkPivotAddSeconds(SignalPhase signalPhase, int seconds)
        {
            signalPhase.ResetVolume();
            foreach (var cyclePcd in signalPhase.Cycles)
            {
                cyclePcd.AddSecondsToDetectorEvents(seconds);
            }
        }

        private SignalPhase GetSignalPhaseData(
            DateTime start,
            DateTime end,
            bool getPermissivePhase,
            bool showVolume,
            int? pcdCycleTime,
            int binSize,
            int metricTypeId,
            Approach approach)
        {
            var detectorEvents = GetDetectorEvents(metricTypeId, approach, start, end);
            var cycles = cycleService.GetPcdCycles(start, end, approach, detectorEvents, getPermissivePhase, pcdCycleTime);
            var plans = planService.GetPcdPlans(cycles, start, end, approach);
            return new SignalPhase(
                showVolume ? SetVolume(detectorEvents, binSize, start, end) : null,
                plans,
                cycles,
                detectorEvents,
                approach,
                start,
                end
                );
        }

        private List<ControllerEventLog> GetDetectorEvents(
            int metricTypeId, 
            Approach approach, 
            DateTime start, 
            DateTime end)
        {
                var events = new List<ControllerEventLog>();
                var detectorsForMetric = approach.GetDetectorsForMetricType(metricTypeId);
                foreach (var d in detectorsForMetric)
                    events.AddRange(controllerEventLogRepository.GetEventsByEventCodesParam(
                        approach.SignalId, 
                        start,
                        end, 
                        new List<int> {82}, 
                        d.DetChannel, 
                        d.GetOffset(), 
                        d.LatencyCorrection));
            return events;
        }


        private VolumeCollection SetVolume(List<ControllerEventLog> detectorEvents, int binSize, DateTime start, DateTime end)
        {
            return new VolumeCollection(start, end, detectorEvents, binSize);
        }
    }
}