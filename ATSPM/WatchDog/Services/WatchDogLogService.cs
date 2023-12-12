using ATSPM.Application.Repositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.TempExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;

namespace WatchDog.Services
{
    public partial class WatchDogLogService
    {
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly AnalysisPhaseCollectionService analysisPhaseCollectionService;
        private readonly PhaseService phaseService;
        private readonly ILogger<WatchDogLogService> logger;

        public WatchDogLogService(IControllerEventLogRepository controllerEventLogRepository,
            AnalysisPhaseCollectionService analysisPhaseCollectionService,
            PhaseService phaseService,
            ILogger<WatchDogLogService> logger)
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.analysisPhaseCollectionService = analysisPhaseCollectionService;
            this.phaseService = phaseService;
            this.logger = logger;
        }

        public async Task<List<WatchDogLogEvent>> GetWatchDogIssues(
            LoggingOptions options,
            List<Location> signals)
        {
            if (signals.IsNullOrEmpty())
            {
                return new List<WatchDogLogEvent>();
            }
            else
            {
                var errors = new ConcurrentBag<WatchDogLogEvent>();

                foreach (var signal in signals)//.Where(s => s.locationIdentifier == "7115"))
                {
                    var signalEvents = controllerEventLogRepository.GetSignalEventsBetweenDates(
                        signal.LocationIdentifier,
                        options.AnalysisStart,
                        options.AnalysisEnd).ToList();
                    var recordsError = await CheckSignalRecordCount(options.ScanDate, signal, options, signalEvents);
                    if (recordsError != null)
                    {
                        errors.Add(recordsError);
                        continue;
                    }
                    var tasks = new List<Task>();
                    tasks.Add(CheckSignalForPhaseErrors(signal, options, signalEvents, errors));
                    tasks.Add(CheckDetectors(signal, options, signalEvents, errors));
                    //CheckApplicationEvents(signals, options);
                    await Task.WhenAll(tasks);
                }
                return errors.ToList();
            }
        }

        private async Task CheckDetectors(Location signal, LoggingOptions options, List<ControllerEventLog> signalEvents, ConcurrentBag<WatchDogLogEvent> errors)
        {
            var detectorEventCodes = new List<int> { 81, 82 };
            CheckForUnconfiguredDetectors(signal, options, signalEvents, errors, detectorEventCodes);
            CheckForLowDetectorHits(signal, options, signalEvents, errors, detectorEventCodes);
        }

        private void CheckForLowDetectorHits(Location signal, LoggingOptions options, List<ControllerEventLog> signalEvents, ConcurrentBag<WatchDogLogEvent> errors, List<int> detectorEventCodes)
        {
            var detectors = signal.GetDetectorsForSignalThatSupportMetric(6);
            //Parallel.ForEach(detectors, options, detector =>
            foreach (var detector in detectors)
                try
                {
                    if (detector.DetectionTypes != null && detector.DetectionTypes.Any(d => d.Id == ATSPM.Data.Enums.DetectionTypes.AC))
                    {
                        var channel = detector.DetectorChannel;
                        var direction = detector.Approach.DirectionType.Description;
                        var start = new DateTime();
                        var end = new DateTime();
                        if (options.WeekdayOnly && options.ScanDate.DayOfWeek == DayOfWeek.Monday)
                        {
                            start = options.ScanDate.AddDays(-3).Date.AddHours(options.PreviousDayPMPeakStart);
                            end = options.ScanDate.AddDays(-3).Date.AddHours(options.PreviousDayPMPeakEnd);
                        }
                        else
                        {
                            start = options.ScanDate.AddDays(-1).Date.AddHours(options.PreviousDayPMPeakStart);
                            end = options.ScanDate.AddDays(-1).Date.AddHours(options.PreviousDayPMPeakEnd);
                        }
                        var currentVolume = signalEvents.Where(e => e.EventParam == detector.DetectorChannel && detectorEventCodes.Contains(e.EventCode)).Count();
                        //Compare collected hits to low hit threshold, 
                        if (currentVolume < Convert.ToInt32(options.LowHitThreshold))
                        {
                            var error = new WatchDogLogEvent(
                                signal.Id,
                                signal.LocationIdentifier,
                                options.ScanDate,
                                WatchDogComponentType.Detector,
                                detector.Id,
                                WatchDogIssueType.LowDetectorHits,
                                $"CH: {channel} - Count: {currentVolume.ToString().ToLowerInvariant()}",
                                null);
                            if (!errors.Contains(error))
                                errors.Add(error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError($"CheckForLowDetectorHits {detector.Id} {ex.Message}");
                }
        }

        private static void CheckForUnconfiguredDetectors(Location signal, LoggingOptions options, List<ControllerEventLog> signalEvents, ConcurrentBag<WatchDogLogEvent> errors, List<int> detectorEventCodes)
        {
            var detectorChannelsFromEvents = signalEvents.Where(e => detectorEventCodes.Contains(e.EventCode)).Select(e => e.EventParam).Distinct().ToList();
            var detectorChannelsFromDetectors = signal.GetDetectorsForSignal().Select(d => d.DetectorChannel).Distinct().ToList();
            foreach (var channel in detectorChannelsFromEvents)
            {
                if (!detectorChannelsFromDetectors.Contains(channel))
                {
                    var error = new WatchDogLogEvent(
                                signal.Id,
                                signal.LocationIdentifier,
                                options.ScanDate,
                                WatchDogComponentType.Detector,
                                -1,
                                WatchDogIssueType.UnconfiguredDetector,
                                $"Unconfigured detector channel-{channel}",
                                null);
                    if (!errors.Contains(error))
                        errors.Add(error);
                }
            }
        }

        private async Task CheckSignalForPhaseErrors(
            Location signal,
            LoggingOptions options,
            List<ControllerEventLog> signalEvents,
            ConcurrentBag<WatchDogLogEvent> errors)
        {
            var planEvents = signalEvents.GetPlanEvents(
            options.AnalysisStart,
            options.AnalysisEnd).ToList();
            //Do we want to use the ped events extension here?
            var pedEvents = signalEvents.Where(e =>
                new List<int> { 21, 23 }.Contains(e.EventCode)
                && e.Timestamp >= options.AnalysisStart
                && e.Timestamp <= options.AnalysisEnd).ToList();
            var cycleEvents = signalEvents.Where(e =>
                new List<int> { 1, 8, 11 }.Contains(e.EventCode)
                && e.Timestamp >= options.AnalysisStart
                && e.Timestamp <= options.AnalysisEnd).ToList();
            var splitsEventCodes = new List<int>();
            for (var i = 130; i <= 151; i++)
                splitsEventCodes.Add(i);
            var splitsEvents = signalEvents.Where(e =>
                splitsEventCodes.Contains(e.EventCode)
                && e.Timestamp >= options.AnalysisStart
                && e.Timestamp <= options.AnalysisEnd).ToList();
            var terminationEvents = signalEvents.Where(e =>
            new List<int> { 4, 5, 6, 7 }.Contains(e.EventCode)
            && e.Timestamp >= options.AnalysisStart
            && e.Timestamp <= options.AnalysisEnd).ToList();
            signalEvents = null;

            CheckForUnconfiguredApproaches(signal, options, errors, cycleEvents);

            AnalysisPhaseCollectionData analysisPhaseCollection = null;
            try
            {
                analysisPhaseCollection = analysisPhaseCollectionService.GetAnalysisPhaseCollectionData(
                    signal.LocationIdentifier,
                    options.AnalysisStart,
                    options.AnalysisEnd,
                    planEvents,
                    cycleEvents,
                    splitsEvents,
                    pedEvents,
                    terminationEvents,
                    signal,
                    options.ConsecutiveCount);

            }
            catch (Exception e)
            {
                logger.LogError($"Unable to get analysis phase for signal {signal.LocationIdentifier}");
            }

            if (analysisPhaseCollection != null)
            {
                foreach (var phase in analysisPhaseCollection.AnalysisPhases)
                //Parallel.ForEach(APcollection.Items, options,phase =>
                {
                    var taskList = new List<Task>();
                    var approach = signal.Approaches.Where(a => a.ProtectedPhaseNumber == phase.PhaseNumber).FirstOrDefault();
                    if (approach != null)
                    {
                        try
                        {
                            taskList.Add(CheckForMaxOut(phase, approach, options, errors));
                        }
                        catch (Exception e)
                        {
                            logger.LogError($"{phase.locationIdentifier} {phase.PhaseNumber} - Max Out Error {e.Message}");
                        }

                        try
                        {

                            taskList.Add(CheckForForceOff(phase, approach, options, errors));
                        }
                        catch (Exception e)
                        {
                            logger.LogError($"{phase.locationIdentifier} {phase.PhaseNumber} - Force Off Error {e.Message}");
                        }

                        try
                        {
                            taskList.Add(CheckForStuckPed(phase, approach, options, errors));
                        }
                        catch (Exception e)
                        {
                            logger.LogError($"{phase.locationIdentifier} {phase.PhaseNumber} - Stuck Ped Error {e.Message}");
                        }
                    }
                    await Task.WhenAll(taskList);
                }
            }
        }

        private void CheckForUnconfiguredApproaches(Location signal, LoggingOptions options, ConcurrentBag<WatchDogLogEvent> errors, List<ControllerEventLog> cycleEvents)
        {
            var phasesInUse = cycleEvents.Where(d => d.EventCode == 1).Select(d => d.EventParam).Distinct();
            foreach (var phaseNumber in phasesInUse)
            {
                var phase = phaseService.GetPhases(signal).Find(p => p.PhaseNumber == phaseNumber);
                if (phase == null)
                {
                    var error = new WatchDogLogEvent
                    (
                        signal.Id,
                        signal.LocationIdentifier,
                        options.ScanDate,
                        WatchDogComponentType.Approach,
                        -1,
                        WatchDogIssueType.UnconfiguredApproach,
                        $"No corresponding approach configured",
                        phaseNumber
                    );
                    if (!errors.Contains(error))
                    {
                        logger.LogDebug($"Signal {signal.LocationIdentifier} {phaseNumber} Not Configured");
                        errors.Add(error);
                    }
                }
            }
        }

        private async Task CheckForStuckPed(AnalysisPhaseData phase, Approach approach, LoggingOptions options, ConcurrentBag<WatchDogLogEvent> errors)
        {
            if (phase.PedestrianEvents.Count > options.MaximumPedestrianEvents)
            {
                var error = new WatchDogLogEvent
                (
                    approach.Location.Id,
                    approach.Location.LocationIdentifier,
                    options.ScanDate,
                    WatchDogComponentType.Approach,
                    approach.Id,
                    WatchDogIssueType.StuckPed,
                    phase.PedestrianEvents.Count + " Pedestrian Activations",
                    phase.PhaseNumber
                );
                if (!errors.Contains(error))
                {
                    logger.LogDebug($"Signal {approach.Location.LocationIdentifier} {phase.PedestrianEvents.Count} Pedestrian Activations");
                    errors.Add(error);
                }
            }
        }

        private async Task CheckForForceOff(AnalysisPhaseData phase, Approach approach, LoggingOptions options, ConcurrentBag<WatchDogLogEvent> errors)
        {
            if (phase.PercentForceOffs > options.PercentThreshold &&
                phase.TerminationEvents.Where(t => t.EventCode != 7).Count() > options.MinPhaseTerminations)
            {
                var error = new WatchDogLogEvent
                (
                    approach.Location.Id,
                    approach.Location.LocationIdentifier,
                    options.ScanDate,
                    WatchDogComponentType.Approach,
                    approach.Id,
                    WatchDogIssueType.ForceOffThreshold,
                    "Force Offs " + Math.Round(phase.PercentForceOffs * 100, 1) + "%",
                    phase.PhaseNumber
                );
                if (!errors.Contains(error))
                {
                    logger.LogDebug($"Signal {approach.Location.LocationIdentifier} Has ForceOff Errors");
                    errors.Add(error);
                }
            }
        }

        private async Task CheckForMaxOut(AnalysisPhaseData phase, Approach approach, LoggingOptions options, ConcurrentBag<WatchDogLogEvent> errors)
        {
            if (phase.PercentMaxOuts > options.PercentThreshold &&
                phase.TotalPhaseTerminations > options.MinPhaseTerminations)
            {
                var error = new WatchDogLogEvent
                (
                    approach.Location.Id,
                    approach.Location.LocationIdentifier,
                    options.ScanDate,
                    WatchDogComponentType.Approach,
                    approach.Id,
                    WatchDogIssueType.MaxOutThreshold,
                    "Max Outs " + Math.Round(phase.PercentMaxOuts * 100, 1) + "%",
                    phase.PhaseNumber
                );
                if (errors.Count == 0 || !errors.Contains(error))
                {
                    logger.LogDebug($"Signal {approach.Location.LocationIdentifier} Has MaxOut Errors");
                    errors.Add(error);
                }
            }
        }

        private async Task<WatchDogLogEvent> CheckSignalRecordCount(DateTime dateToCheck, Location signal, LoggingOptions options, List<ControllerEventLog> signalEvents)
        {
            if (signalEvents.Count > options.MinimumRecords)
            {
                logger.LogDebug($"Signal {signal.LocationIdentifier} has {signalEvents.Count} records");
                return null;
            }
            else
            {
                logger.LogDebug($"Signal {signal.LocationIdentifier} Does Not Have Sufficient records");
                return new WatchDogLogEvent(
                    signal.Id,
                    signal.LocationIdentifier,
                    options.ScanDate,
                    WatchDogComponentType.Signal,
                    signal.Id,
                    WatchDogIssueType.RecordCount,
                    "Missing Records - IP: " + signal.Ipaddress,
                    null
                );
            }

        }
    }
}