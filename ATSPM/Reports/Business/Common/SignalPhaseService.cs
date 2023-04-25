using ATSPM.Application.Repositories;
using ATSPM.Application.Exceptions;
using ATSPM.Data.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace ATSPM.Application.Reports.Business.Common
{
    public class SignalPhaseService
    {
        private readonly PlanService planService;
        private readonly CycleService cycleService;
        private readonly ILogger logger;

        public SignalPhaseService(
            PlanService planService,
            CycleService cycleService,
            ILogger logger
            )
        {
            this.planService = planService;
            this.cycleService = cycleService;
            this.logger = logger;
        }



        public void LinkPivotAddSeconds(SignalPhase signalPhase, int seconds)
        {
            signalPhase.ResetVolume();
            foreach (var cyclePcd in signalPhase.Cycles)
            {
                cyclePcd.AddSecondsToDetectorEvents(seconds);
            }
        }

        /// <summary>
        /// Needs event codes 1,8,9,61,63,64,131,82
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="getPermissivePhase"></param>
        /// <param name="showVolume"></param>
        /// <param name="pcdCycleTime"></param>
        /// <param name="binSize"></param>
        /// <param name="approach"></param>
        /// <param name="events"></param>
        /// <returns></returns>
        public SignalPhase GetSignalPhaseData(
            DateTime start,
            DateTime end,
            bool showVolume,
            int? pcdCycleTime,
            int binSize,
            Approach approach,
            List<ControllerEventLog> cycleEvents,
            List<ControllerEventLog> planEvents,
            List<ControllerEventLog> detectorEvents)
        {
            if (approach == null)
            {
                logger.LogError("Approach cannot be null");
                throw new ReportsNullAgrumentException("Approach cannot be null");
            }

            if (cycleEvents.IsNullOrEmpty())
                return new SignalPhase();
            var cycles = cycleService.GetPcdCycles(start, end, approach, detectorEvents, cycleEvents, pcdCycleTime);
            var plans = planService.GetPcdPlans(cycles, start, end, approach, planEvents);
            return new SignalPhase(
                showVolume ? new VolumeCollection(start, end, detectorEvents, binSize) : null,
                plans,
                cycles,
                detectorEvents,
                approach,
                start,
                end
                );
        }
    }
}