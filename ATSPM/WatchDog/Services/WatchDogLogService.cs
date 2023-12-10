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
        private readonly ILogger<WatchDogLogService> logger;

        public WatchDogLogService(IControllerEventLogRepository controllerEventLogRepository,
            AnalysisPhaseCollectionService analysisPhaseCollectionService,
            ILogger<WatchDogLogService> logger)
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.analysisPhaseCollectionService = analysisPhaseCollectionService;
            this.logger = logger;
        }

        public async Task<List<WatchDogLogEvent>> GetWatchDogIssues(
            LoggingOptions options,
            List<Signal> signals)
        {
            if (signals.IsNullOrEmpty())
            {
                return new List<WatchDogLogEvent>();
            }
            else
            {
                var errors = new ConcurrentBag<WatchDogLogEvent>();

                foreach (var signal in signals)//.Where(s => s.SignalIdentifier == "7115"))
                {
                    var signalEvents = controllerEventLogRepository.GetSignalEventsBetweenDates(
                        signal.SignalIdentifier,
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
                    tasks.Add(CheckForLowDetectorHits(signal, options, signalEvents, errors));
                    //CheckApplicationEvents(signals, options);
                    await Task.WhenAll(tasks);
                }
                return errors.ToList();
            }
        }

        private async Task CheckForLowDetectorHits(Signal signal, LoggingOptions options, List<ControllerEventLog> signalEvents, ConcurrentBag<WatchDogLogEvent> errors)
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
                        var detectorEventCodes = new List<int> { 81, 82 };
                        var currentVolume = signalEvents.Where(e => e.EventParam == detector.DetectorChannel && detectorEventCodes.Contains(e.EventCode)).Count();
                        //Compare collected hits to low hit threshold, 
                        if (currentVolume < Convert.ToInt32(options.LowHitThreshold))
                        {
                            var error = new WatchDogLogEvent(
                                signal.Id,
                                signal.SignalIdentifier,
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

        private async Task CheckSignalForPhaseErrors(
            Signal signal,
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

            AnalysisPhaseCollectionData analysisPhaseCollection = null;
            try
            {
                analysisPhaseCollection = analysisPhaseCollectionService.GetAnalysisPhaseCollectionData(
                    signal.SignalIdentifier,
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
                logger.LogError($"Unable to get analysis phase for signal {signal.SignalIdentifier}");
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
                            logger.LogError($"{phase.SignalIdentifier} {phase.PhaseNumber} - Max Out Error {e.Message}");
                        }

                        try
                        {

                            taskList.Add(CheckForForceOff(phase, approach, options, errors));
                        }
                        catch (Exception e)
                        {
                            logger.LogError($"{phase.SignalIdentifier} {phase.PhaseNumber} - Force Off Error {e.Message}");
                        }

                        try
                        {
                            taskList.Add(CheckForStuckPed(phase, approach, options, errors));
                        }
                        catch (Exception e)
                        {
                            logger.LogError($"{phase.SignalIdentifier} {phase.PhaseNumber} - Stuck Ped Error {e.Message}");
                        }
                    }
                    await Task.WhenAll(taskList);
                }
            }
        }

        private async Task CheckForStuckPed(AnalysisPhaseData phase, Approach approach, LoggingOptions options, ConcurrentBag<WatchDogLogEvent> errors)
        {
            if (phase.PedestrianEvents.Count > options.MaximumPedestrianEvents)
            {
                var error = new WatchDogLogEvent
                (
                    approach.Signal.Id,
                    approach.Signal.SignalIdentifier,
                    options.ScanDate,
                    WatchDogComponentType.Approach,
                    approach.Id,
                    WatchDogIssueType.StuckPed,
                    phase.PedestrianEvents.Count + " Pedestrian Activations",
                    phase.PhaseNumber
                );
                if (!errors.Contains(error))
                {
                    logger.LogDebug($"Signal {approach.Signal.SignalIdentifier} {phase.PedestrianEvents.Count} Pedestrian Activations");
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
                    approach.Signal.Id,
                    approach.Signal.SignalIdentifier,
                    options.ScanDate,
                    WatchDogComponentType.Approach,
                    approach.Id,
                    WatchDogIssueType.ForceOffThreshold,
                    "Force Offs " + Math.Round(phase.PercentForceOffs * 100, 1) + "%",
                    phase.PhaseNumber
                );
                if (!errors.Contains(error))
                {
                    logger.LogDebug($"Signal {approach.Signal.SignalIdentifier} Has ForceOff Errors");
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
                    approach.Signal.Id,
                    approach.Signal.SignalIdentifier,
                    options.ScanDate,
                    WatchDogComponentType.Approach,
                    approach.Id,
                    WatchDogIssueType.MaxOutThreshold,
                    "Max Outs " + Math.Round(phase.PercentMaxOuts * 100, 1) + "%",
                    phase.PhaseNumber
                );
                if (errors.Count == 0 || !errors.Contains(error))
                {
                    logger.LogDebug($"Signal {approach.Signal.SignalIdentifier} Has MaxOut Errors");
                    errors.Add(error);
                }
            }
        }

        private async Task<WatchDogLogEvent> CheckSignalRecordCount(DateTime dateToCheck, Signal signal, LoggingOptions options, List<ControllerEventLog> signalEvents)
        {
            if (signalEvents.Count > options.MinimumRecords)
            {
                logger.LogDebug($"Signal {signal.SignalIdentifier} has {signalEvents.Count} records");
                return null;
            }
            else
            {
                logger.LogDebug($"Signal {signal.SignalIdentifier} Does Not Have Sufficient records");
                return new WatchDogLogEvent(
                    signal.Id,
                    signal.SignalIdentifier,
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