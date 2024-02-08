using ATSPM.Application.Extensions;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.TempExtensions;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.ReportApi.Business.SplitFail
{
    public class SplitFailPhaseData
    {
        public List<SplitFailDetectorActivation> DetectorActivations = new List<SplitFailDetectorActivation>();
        public List<SplitFailBin> Bins { get; set; } = new List<SplitFailBin>();
        public int TotalFails { get; set; }
        public Approach Approach { get; set; }
        public bool GetPermissivePhase { get; set; }
        public List<CycleSplitFail> Cycles { get; set; }
        public List<PlanSplitFail> Plans { get; set; }
        public Dictionary<string, string> Statistics { get; set; }
        public string PhaseNumberSort { get; set; }
    }

    public class SplitFailPhaseService
    {
        private readonly CycleService cycleService;
        private readonly PlanService planService;

        public SplitFailPhaseService(
            CycleService cycleService,
            PlanService planService
            )
        {
            this.cycleService = cycleService;
            this.planService = planService;
        }

        public SplitFailPhaseData GetSplitFailPhaseData(
            SplitFailOptions options,
            IReadOnlyList<ControllerEventLog> cycleEvents,
            IReadOnlyList<ControllerEventLog> planEvents,
            IReadOnlyList<ControllerEventLog> terminationEvents,
            IReadOnlyList<ControllerEventLog> detectorEvents,
            Approach approach)
        {
            var splitFailPhaseData = new SplitFailPhaseData();
            splitFailPhaseData.Approach = approach;
            splitFailPhaseData.GetPermissivePhase = options.UsePermissivePhase;
            splitFailPhaseData.PhaseNumberSort = options.UsePermissivePhase ? approach.PermissivePhaseNumber.Value.ToString() + "-1" : approach.ProtectedPhaseNumber.ToString() + "-2";
            splitFailPhaseData.Cycles = cycleService.GetSplitFailCycles(
                options,
                cycleEvents,
                terminationEvents);
            SetDetectorActivations(options, splitFailPhaseData, detectorEvents);
            AddDetectorActivationsToCycles(splitFailPhaseData);
            splitFailPhaseData.Plans = planService.GetSplitFailPlans(
                splitFailPhaseData.Cycles,
                options,
                splitFailPhaseData.Approach,
                planEvents);
            splitFailPhaseData.TotalFails = splitFailPhaseData.Cycles.Count(c => c.IsSplitFail);
            splitFailPhaseData.Statistics = new Dictionary<string, string>();
            splitFailPhaseData.Statistics.Add("Total Split Failures", splitFailPhaseData.TotalFails.ToString());
            SetBinStatistics(options, splitFailPhaseData);
            return splitFailPhaseData;
        }

        private void AddDetectorActivationsToCycles(SplitFailPhaseData splitFailPhaseData)
        {
            foreach (var cycleSplitFail in splitFailPhaseData.Cycles)
                cycleSplitFail.SetDetectorActivations(splitFailPhaseData.DetectorActivations);
        }

        private void SetBinStatistics(SplitFailOptions options, SplitFailPhaseData splitFailPhaseData)
        {
            var startTime = options.Start;
            do
            {
                var endTime = startTime.AddMinutes(15);
                var cycles = splitFailPhaseData.Cycles.Where(c => c.StartTime >= startTime && c.StartTime <= endTime).ToList();
                splitFailPhaseData.Bins.Add(new SplitFailBin(startTime, endTime, cycles));
                startTime = startTime.AddMinutes(15);
            } while (startTime < options.End);
        }

        private void CombineDetectorActivations(SplitFailPhaseData splitFailPhaseData)
        {
            var tempDetectorActivations = new List<SplitFailDetectorActivation>();

            //look at every item in the original list
            foreach (var current in splitFailPhaseData.DetectorActivations)
                if (!current.ReviewedForOverlap)
                {
                    var overlapingActivations = splitFailPhaseData.DetectorActivations.Where(d => d.ReviewedForOverlap == false &&
                        (
                            //   if it starts after current starts  and    starts before current ends      and    end after current ends   
                            d.DetectorOn >=
                            current.DetectorOn &&
                            d.DetectorOn <=
                            current.DetectorOff &&
                            d.DetectorOff >= current.DetectorOff
                            //OR if it starts BEFORE curent starts  and ends AFTER current starts           and ends BEFORE current ends
                            || d.DetectorOn <=
                            current.DetectorOn &&
                            d.DetectorOff >=
                            current.DetectorOn &&
                            d.DetectorOff <= current.DetectorOff
                            //OR if it starts AFTER current starts   and it ends BEFORE current ends
                            || d.DetectorOn >=
                            current.DetectorOn &&
                            d.DetectorOff <= current.DetectorOff
                            //OR if it starts BEFORE current starts  and it ens AFTER current ends 
                            || d.DetectorOn <=
                            current.DetectorOn &&
                            d.DetectorOff >= current.DetectorOff
                        )
                    //then add it to the overlap list
                    ).ToList();

                    //if there are any in the list (and here should be at least one that matches current)
                    if (overlapingActivations.Any())
                    {
                        //Then make a new activation that starts witht eh earliest start and ends with the latest end
                        var tempDetectorActivation = new SplitFailDetectorActivation
                        {
                            DetectorOn = overlapingActivations.Min(o => o.DetectorOn),
                            DetectorOff = overlapingActivations.Max(o => o.DetectorOff)
                        };
                        //and add the new one to a temp list
                        tempDetectorActivations.Add(tempDetectorActivation);

                        //mark everything in the  overlap list as Reviewed
                        foreach (var splitFailDetectorActivation in overlapingActivations)
                            splitFailDetectorActivation.ReviewedForOverlap = true;
                    }
                }

            //since we went through every item in the original list, if there were no overlaps, it shoudl equal the temp list
            if (splitFailPhaseData.DetectorActivations.Count != tempDetectorActivations.Count)
            {
                //if the counts do not match, we have to set the original list to the temp list and try the combinaitons again
                splitFailPhaseData.DetectorActivations = tempDetectorActivations;
                CombineDetectorActivations(splitFailPhaseData);
            }
        }

        private void SetDetectorActivations(
            SplitFailOptions options,
            SplitFailPhaseData splitFailPhaseData,
            IReadOnlyList<ControllerEventLog> detectorEvents)
        {
            var phaseNumber = splitFailPhaseData.GetPermissivePhase ? splitFailPhaseData.Approach.PermissivePhaseNumber.Value : splitFailPhaseData.Approach.ProtectedPhaseNumber;
            var detectors = splitFailPhaseData.Approach.GetAllDetectorsOfDetectionType(Data.Enums.DetectionTypes.SBP);// .GetDetectorsForMetricType(12);

            foreach (var detector in detectors)
            {
                var events = detectorEvents.Where(d => d.EventParam == detector.DetectorChannel).ToList();
                //TODO: Verify that this is not needed
                //if (!events.Any())
                //{
                //    CheckForDetectorOnBeforeStart(options, detector, splitFailPhaseData);
                //}
                //else
                //{
                AddDetectorOnToBeginningIfNecessary(options, detector, events);
                AddDetectorOffToEndIfNecessary(options, detector, events);
                AddDetectorActivationsFromList(events, splitFailPhaseData);
                //}
            }
            CombineDetectorActivations(splitFailPhaseData);
        }

        private void AddDetectorActivationsFromList(List<ControllerEventLog> events, SplitFailPhaseData splitFailPhaseData)
        {
            events = events.OrderBy(e => e.Timestamp).ToList();
            for (var i = 0; i < events.Count - 1; i++)
                if (events[i].EventCode == 82 && events[i + 1].EventCode == 81)
                    splitFailPhaseData.DetectorActivations.Add(new SplitFailDetectorActivation
                    {
                        DetectorOn = events[i].Timestamp,
                        DetectorOff = events[i + 1].Timestamp
                    });
        }

        private static void AddDetectorOffToEndIfNecessary(SplitFailOptions options, Detector detector,
            List<ControllerEventLog> events)
        {
            if (events.LastOrDefault()?.EventCode == 82)
                events.Insert(events.Count, new ControllerEventLog
                {
                    Timestamp = options.End,
                    EventCode = 81,
                    EventParam = detector.DetectorChannel,
                    SignalIdentifier = options.locationIdentifier
                });
        }

        private static void AddDetectorOnToBeginningIfNecessary(SplitFailOptions options, Detector detector,
            List<ControllerEventLog> events)
        {
            if (events.FirstOrDefault()?.EventCode == 81)
                events.Insert(0, new ControllerEventLog
                {
                    Timestamp = options.Start,
                    EventCode = 82,
                    EventParam = detector.DetectorChannel,
                    SignalIdentifier = options.locationIdentifier
                });
        }
    }
}