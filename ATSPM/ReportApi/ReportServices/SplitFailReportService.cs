﻿using ATSPM.Application.Business;
using ATSPM.Application.Business.Common;
using ATSPM.Application.Business.SplitFail;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Application.TempExtensions;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Split fail report service
    /// </summary>
    public class SplitFailReportService : ReportServiceBase<SplitFailOptions, IEnumerable<SplitFailsResult>>
    {
        private readonly SplitFailPhaseService splitFailPhaseService;
        private readonly IIndianaEventLogRepository controllerEventLogRepository;
        private readonly ILocationRepository LocationRepository;
        private readonly PhaseService phaseService;

        /// <inheritdoc/>
        public SplitFailReportService(
            SplitFailPhaseService splitFailPhaseService,
            IIndianaEventLogRepository controllerEventLogRepository,
            ILocationRepository LocationRepository,
            PhaseService phaseService
            )
        {
            this.splitFailPhaseService = splitFailPhaseService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.LocationRepository = LocationRepository;
            this.phaseService = phaseService;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<SplitFailsResult>> ExecuteAsync(SplitFailOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var Location = LocationRepository.GetLatestVersionOfLocation(parameter.LocationIdentifier, parameter.Start);

            if (Location == null)
            {
                //return BadRequest("Location not found");
                return await Task.FromException<IEnumerable<SplitFailsResult>>(new NullReferenceException("Location not found"));
            }

            var controllerEventLogs = controllerEventLogRepository.GetEventsBetweenDates(Location.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();

            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for Location");
                return await Task.FromException<IEnumerable<SplitFailsResult>>(new NullReferenceException("No Controller Event Logs found for Location"));
            }

            var planEvents = controllerEventLogs.GetPlanEvents(
            parameter.Start.AddHours(-12),
               parameter.End.AddHours(12)).ToList();
            var phaseDetails = phaseService.GetPhases(Location);
            var tasks = new List<Task<IEnumerable<SplitFailsResult>>>();
            foreach (var phase in phaseDetails)
            {
                tasks.Add(GetChartDataForApproach(parameter, phase, controllerEventLogs, planEvents));
            }

            var results = await Task.WhenAll(tasks);
            var finalResultcheck = results.Where(result => result != null).SelectMany(r => r).OrderBy(r => r.PhaseNumber).ToList();

            //if (finalResultcheck.IsNullOrEmpty())
            //{
            //    return Ok("No chart data found");
            //}
            //return Ok(finalResultcheck);

            return finalResultcheck;
        }

        private async Task<IEnumerable<SplitFailsResult>> GetChartDataForApproach(
            SplitFailOptions options,
            PhaseDetail phaseDetail,
            List<IndianaEvent> controllerEventLogs,
            List<IndianaEvent> planEvents)
        {
            //var cycleEventCodes = approach.GetCycleEventCodes(options.UsePermissivePhase);
            var cycleEvents = controllerEventLogs.GetCycleEventsWithTimeExtension(
                phaseDetail.PhaseNumber,
                phaseDetail.UseOverlap,
                options.Start,
                options.End);
            if (cycleEvents.IsNullOrEmpty())
                return null;
            var terminationEvents = controllerEventLogs.GetEventsByEventCodes(
                 options.Start,
                 options.End,
                 new List<short>
                 {
                     4,
                     5,
                     6
                 },
                 phaseDetail.PhaseNumber);
            var detectors = phaseDetail.Approach.GetDetectorsForMetricType(options.MetricTypeId);
            var tasks = new List<Task<SplitFailsResult>>();
            foreach (var detectionType in detectors.SelectMany(d => d.DetectionTypes).Distinct())
            {
                tasks.Add(GetChartDataByDetectionType(options, phaseDetail, controllerEventLogs, planEvents, cycleEvents, terminationEvents, detectors, detectionType));
            }
            var results = await Task.WhenAll(tasks);
            return results.Where(result => result != null).OrderBy(r => r.PhaseNumber);
        }

        private async Task<SplitFailsResult> GetChartDataByDetectionType(
            SplitFailOptions options,
            PhaseDetail phaseDetail,
            List<IndianaEvent> controllerEventLogs,
            List<IndianaEvent> planEvents,
            IReadOnlyList<IndianaEvent> cycleEvents,
            IReadOnlyList<IndianaEvent> terminationEvents,
            List<Detector> detectors,
            DetectionType detectionType)
        {
            var tempDetectorEvents = controllerEventLogs.GetDetectorEvents(
               options.MetricTypeId,
               phaseDetail.Approach,
               options.Start,
               options.End,
               true,
               true,
               detectionType);
            if (tempDetectorEvents == null)
            {
                return null;
            }
            var detectorEvents = tempDetectorEvents.ToList();
            AddBeginEndEventsByDetector(options, detectors, detectionType, detectorEvents);
            var splitFailData = splitFailPhaseService.GetSplitFailPhaseData(
                options,
                cycleEvents,
                planEvents,
                terminationEvents,
                detectorEvents,
                phaseDetail.Approach,
                phaseDetail.IsPermissivePhase);
            var result = new SplitFailsResult(
                options.LocationIdentifier,
                phaseDetail.Approach.Id,
                phaseDetail.PhaseNumber,
                splitFailData.GetPermissivePhase ? "Permissive" : "Protected",
                options.Start,
                options.End,
                splitFailData.TotalFails,
                splitFailData.Plans,
                splitFailData.Cycles.Where(c => c.IsSplitFail).Select(c => new DataPointBase(c.StartTime)).ToList(),
                splitFailData.Cycles
                    .Where(c => c.TerminationEvent == CycleSplitFail.TerminationType.GapOut)
                    .Select(b => new DataPointForDouble(b.StartTime, b.GreenOccupancyPercent)).ToList(),
                splitFailData.Cycles
                    .Where(c => c.TerminationEvent == CycleSplitFail.TerminationType.GapOut)
                    .Select(b => new DataPointForDouble(b.StartTime, b.RedOccupancyPercent)).ToList(),
                splitFailData.Cycles
                    .Where(c => c.TerminationEvent == CycleSplitFail.TerminationType.ForceOff)
                    .Select(b => new DataPointForDouble(b.StartTime, b.GreenOccupancyPercent)).ToList(),
                splitFailData.Cycles
                    .Where(c => c.TerminationEvent == CycleSplitFail.TerminationType.ForceOff)
                    .Select(b => new DataPointForDouble(b.StartTime, b.RedOccupancyPercent)).ToList(),
                splitFailData.Bins.Select(b => new DataPointForDouble(b.StartTime, b.AverageGreenOccupancyPercent)).ToList(),
                splitFailData.Bins.Select(b => new DataPointForDouble(b.StartTime, b.AverageRedOccupancyPercent)).ToList(),
                splitFailData.Bins.Select(b => new DataPointForDouble(b.StartTime, b.PercentSplitfails)).ToList()
                );
            result.ApproachDescription = phaseDetail.Approach.Description;
            result.LocationDescription = phaseDetail.Approach.Location.LocationDescription();
            return result;
        }

        private static void AddBeginEndEventsByDetector(SplitFailOptions options, List<Detector> detectors, DetectionType detectionType, List<IndianaEvent> detectorEvents)
        {
            foreach (Detector channel in detectors.Where(d => d.DetectionTypes.Contains(detectionType)))
            {
                //add an EC 82 at the beginning if the first EC code is 81
                var firstEvent = detectorEvents.Where(d => d.EventParam == channel.DetectorChannel).FirstOrDefault();
                var lastEvent = detectorEvents.Where(d => d.EventParam == channel.DetectorChannel).LastOrDefault();

                if (firstEvent != null && firstEvent.EventCode == 81)
                {
                    var newDetectorOn = new IndianaEvent();
                    newDetectorOn.LocationIdentifier = options.LocationIdentifier;
                    newDetectorOn.Timestamp = options.Start;
                    newDetectorOn.EventCode = 82;
                    newDetectorOn.EventParam = Convert.ToByte(channel.DetectorChannel);
                    detectorEvents.Add(newDetectorOn);
                }

                //add an EC 81 at the end if the last EC code is 82
                if (lastEvent != null && lastEvent.EventCode == 82)
                {
                    var newDetectorOn = new IndianaEvent();
                    newDetectorOn.LocationIdentifier = options.LocationIdentifier;
                    newDetectorOn.Timestamp = options.End;
                    newDetectorOn.EventCode = 81;
                    newDetectorOn.EventParam = Convert.ToByte(channel.DetectorChannel);
                    detectorEvents.Add(newDetectorOn);
                }
            }
        }
    }
}
