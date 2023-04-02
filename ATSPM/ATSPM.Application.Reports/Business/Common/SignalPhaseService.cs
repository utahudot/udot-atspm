using ATSPM.Application.Repositories;
using ATSPM.Application.Extensions;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Reports.Business.Common
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

        public SignalPhase GetSignalPhaseData(
            DateTime start,
            DateTime end,
            bool getPermissivePhase,
            bool showVolume,
            int? pcdCycleTime,
            int binSize,
            Approach approach,
            List<ControllerEventLog> detectorEvents)
        {
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

       


        private VolumeCollection SetVolume(List<ControllerEventLog> detectorEvents, int binSize, DateTime start, DateTime end)
        {
            return new VolumeCollection(start, end, detectorEvents, binSize);
        }
    }
}