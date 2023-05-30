using ATSPM.Application.Analysis.ApproachVolume;
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Application.Enums;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.PurdueCoordination
{
    public class PerdueCoordinationDiagramResult
    {
        //public PerdueCoordinationDiagramResult(
        //    string chartName,
        //    string signalId,
        //    string signalLocation,
        //    int phaseNumber,
        //    string phaseDescription,
        //    DateTime start,
        //    DateTime end,
        //    int totalOnGreenEvents,
        //    int totalDetectorHits,
        //    double percentArrivalOnGreen,
        //    ICollection<PerdueCoordinationPlan> plans,
        //    ICollection<VolumePerHour> volumePerHour,
        //    ICollection<CyclePcd> cycles)
        //{
        //    ChartName = chartName;
        //    SignalId = signalId;
        //    SignalLocation = signalLocation;
        //    PhaseNumber = phaseNumber;
        //    PhaseDescription = phaseDescription;
        //    Start = start;
        //    End = end;
        //    TotalOnGreenEvents = totalOnGreenEvents;
        //    TotalDetectorHits = totalDetectorHits;
        //    PercentArrivalOnGreen = percentArrivalOnGreen;
        //    Plans = plans;
        //    VolumePerHour = volumePerHour;
        //    Cycles = cycles;
        //}

        //public string ChartName { get; internal set; }
        public string SignalId { get; internal set; }
        //public string SignalLocation { get; internal set; }
        public int PhaseNumber { get; internal set; }
        //public string PhaseDescription { get; internal set; }
        public DateTime Start { get; internal set; }
        public DateTime End { get; internal set; }
        public int TotalOnGreenEvents { get; internal set; }
        public int TotalDetectorHits { get; internal set; }
        public double PercentArrivalOnGreen { get; internal set; }
        //public ICollection<PerdueCoordinationPlan> Plans { get; internal set; }
        //public ICollection<VolumePerHour> VolumePerHour { get; internal set; }
        //public ICollection<CyclePcd> Cycles { get; set; }

    }

    public class CyclePcd : RedToRedCycle
    {
        public double TotalArrivalOnGreen => DetectorEvents.Count(d => d.ArrivalType == ArrivalType.ArrivalOnGreen);
        public double TotalArrivalOnYellow => DetectorEvents.Count(d => d.ArrivalType == ArrivalType.ArrivalOnYellow);
        public double TotalArrivalOnRed => DetectorEvents.Count(d => d.ArrivalType == ArrivalType.ArrivalOnRed);
        public double TotalDelay => DetectorEvents.Sum(d => d.Delay);
        public double TotalVolume => DetectorEvents.Count(d => d.TimeStamp >= StartTime && d.TimeStamp < EndTime);
    }

    //public class CalculateApproachDelay : TransformProcessStepBase<Tuple<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>, IEnumerable<Vehicle>>
    //{
    //    public CalculateApproachDelay(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

    //    protected override Task<IEnumerable<Vehicle>> Process(Tuple<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>> input, CancellationToken cancelToken = default)
    //    {
    //        var result = new List<Vehicle>();

    //        foreach (var v in input.Item1)
    //        {
    //            //TODO: Add phase validation here too!!!
    //            var redCycle = input.Item2?.FirstOrDefault(w => w.SignalId == v.Detector.Approach?.Signal?.SignalId && v.CorrectedTimeStamp >= w.StartTime && v.CorrectedTimeStamp <= w.EndTime);

    //            if (redCycle != null)
    //            {
    //                result.Add(new Vehicle(v, redCycle));
    //            }
    //        }

    //        return Task.FromResult<IEnumerable<Vehicle>>(result);
    //    }
    //}


    public class PurdueCoordinationWorkflow : WorkflowBase<IEnumerable<ControllerEventLog>, ApproachVolumeResult>
    {
        protected JoinBlock<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>> mergeCyclesAndVehicles;

        internal GetDetectorEvents GetDetectorEvents { get; private set; }

        public FilteredPhaseIntervalChanges FilteredPhaseIntervalChanges { get; private set; }

        public FilteredDetectorData FilteredDetectorData { get; private set; }
        public CreateRedToRedCycles CreateRedToRedCycles { get; private set; }
        public IdentifyandAdjustVehicleActivations IdentifyandAdjustVehicleActivations { get; private set; }
        public AssignCyclesToVehicles AssignCyclesToVehicles { get; private set; }
        //public GenerateApproachDelayResults GenerateApproachDelayResults { get; private set; }

        public override void InstantiateSteps()
        {
            FilteredPhaseIntervalChanges = new();
            FilteredDetectorData = new();
            CreateRedToRedCycles = new();
            IdentifyandAdjustVehicleActivations = new();
            mergeCyclesAndVehicles = new();
            AssignCyclesToVehicles = new();
            //GenerateApproachDelayResults = new();

            GetDetectorEvents = new();
        }

        public override void AddStepsToTracker()
        {
            Steps.Add(FilteredPhaseIntervalChanges);
            Steps.Add(FilteredDetectorData);
            Steps.Add(CreateRedToRedCycles);
            Steps.Add(IdentifyandAdjustVehicleActivations);
            Steps.Add(mergeCyclesAndVehicles);
            Steps.Add(AssignCyclesToVehicles);
            //Steps.Add(GenerateApproachDelayResults);

            Steps.Add(GetDetectorEvents);
        }

        public override void LinkSteps()
        {
            Input.LinkTo(FilteredPhaseIntervalChanges, new DataflowLinkOptions() { PropagateCompletion = true });
            Input.LinkTo(FilteredDetectorData, new DataflowLinkOptions() { PropagateCompletion = true });

            FilteredPhaseIntervalChanges.LinkTo(CreateRedToRedCycles, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredDetectorData.LinkTo(GetDetectorEvents, new DataflowLinkOptions() { PropagateCompletion = true });
            GetDetectorEvents.LinkTo(IdentifyandAdjustVehicleActivations, new DataflowLinkOptions() { PropagateCompletion = true });
            IdentifyandAdjustVehicleActivations.LinkTo(mergeCyclesAndVehicles.Target1, new DataflowLinkOptions() { PropagateCompletion = true });
            CreateRedToRedCycles.LinkTo(mergeCyclesAndVehicles.Target2, new DataflowLinkOptions() { PropagateCompletion = true });
            mergeCyclesAndVehicles.LinkTo(AssignCyclesToVehicles, new DataflowLinkOptions() { PropagateCompletion = true });
            //AssignCyclesToVehicles.LinkTo(GenerateApproachDelayResults, new DataflowLinkOptions() { PropagateCompletion = true });
            //GenerateApproachDelayResults.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }

    //HACK: figure this out! can't do this with only one detector because you can't figure out opposing
    internal class GetDetectorEvents : TransformProcessStepBase<IEnumerable<ControllerEventLog>, IEnumerable<Tuple<Detector, IEnumerable<ControllerEventLog>>>>
    {
        public GetDetectorEvents(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<Tuple<Detector, IEnumerable<ControllerEventLog>>>> Process(IEnumerable<ControllerEventLog> input, CancellationToken cancelToken = default)
        {
            var result = input.Where(l => l.EventCode == (int)DataLoggerEnum.DetectorOn)
                .GroupBy(g => g.SignalId)
                .Select(signal => signal.AsEnumerable()
                .GroupBy(g => g.EventParam)
                    .Select(s => Tuple.Create(new Detector()
                    {
                        DetChannel = s.Key,
                        DistanceFromStopBar = 340,
                        LatencyCorrection = 0,
                        Approach = new Approach()
                        {
                            Mph = 45,
                            Signal = new Signal() { SignalId = signal.Key }
                        }
                    }, s.AsEnumerable())))
                .SelectMany(s => s);

            return Task.FromResult(result);
        }
    }
}
