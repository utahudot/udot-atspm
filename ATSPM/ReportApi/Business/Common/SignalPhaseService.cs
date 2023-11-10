using ATSPM.Application.Exceptions;
using ATSPM.Data.Models;
using ATSPM.ReportApi.TempExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATSPM.ReportApi.Business.Common
{
    public class SignalPhaseService
    {
        private readonly PlanService planService;
        private readonly CycleService cycleService;
        private readonly ILogger<SignalPhaseService> logger;

        public SignalPhaseService(
            PlanService planService,
            CycleService cycleService,
            ILogger<SignalPhaseService> logger
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
        public async Task<SignalPhase> GetSignalPhaseData(
            PhaseDetail phaseDetail,
            DateTime start,
            DateTime end,
            bool showVolume,
            int? pcdCycleTime,
            int binSize,
            List<ControllerEventLog> cycleEvents,
            List<ControllerEventLog> planEvents,
            List<ControllerEventLog> detectorEvents)
        {
            if (phaseDetail == null || phaseDetail.Approach == null)
            {
                logger.LogError("Approach cannot be null");
                throw new ArgumentNullException("Approach cannot be null");
            }

            if (!cycleEvents.Any())
                return new SignalPhase();
            var cycles = await cycleService.GetPcdCycles(start, end, detectorEvents, cycleEvents, pcdCycleTime);
            var plans = planService.GetPcdPlans(cycles, start, end, phaseDetail.Approach, planEvents);
            return new SignalPhase(
                showVolume ? new VolumeCollection(start, end, detectorEvents, binSize) : null,
                plans,
                cycles,
                detectorEvents,
                phaseDetail.Approach,
                start,
                end
                );
        }

        public async Task<SignalPhase> GetSignalPhaseData(
            PhaseDetail phaseDetail,
            DateTime start,
            DateTime end,
            int binSize,
            DetectionType detectionType,
            List<ControllerEventLog> controllerEventLogs,
            List<ControllerEventLog> planEvents,
            bool getVolume)
        {
            var detectorEvents = controllerEventLogs.GetDetectorEvents(
                8,
                phaseDetail.Approach,
                start,
                end,
                true,
                false,
                detectionType);
            if (detectorEvents == null)
            {
                return null;
            }

            var cycleEvents = controllerEventLogs.GetCycleEventsWithTimeExtension(
                phaseDetail.PhaseNumber,
                phaseDetail.UseOverlap,
                start,
                end);
            if (cycleEvents.IsNullOrEmpty())
                return null;
            var signalPhase = await GetSignalPhaseData(
                phaseDetail,
                start,
                end,
                getVolume,
                null,
                binSize,
                cycleEvents.ToList(),
                planEvents.ToList(),
                detectorEvents.ToList());
            return signalPhase;
        }
    }
}