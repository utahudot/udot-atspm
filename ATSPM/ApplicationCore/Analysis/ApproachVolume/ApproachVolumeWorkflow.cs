using ATSPM.Application.Analysis.ApproachDelay;
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.ApproachVolume
{
    public class Volume
    {
        public string SignalId { get; set; }
        public int Phase { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int DetectorCount { get; set; }

        public override string ToString()
        {
            return $"{Phase} - {StartTime} - {EndTime} - {DetectorCount}";
        }
    }

    public class CalculatePhaseVolume : TransformProcessStepBase<IEnumerable<CorrectedDetectorEvent>, IReadOnlyList<Volume>>
    {
        public CalculatePhaseVolume(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IReadOnlyList<Volume>> Process(IEnumerable<CorrectedDetectorEvent> input, CancellationToken cancelToken = default)
        {
            throw new NotImplementedException();
        }
    }

    public class ApproachVolumeWorkflow : WorkflowBase<IEnumerable<ControllerEventLog>, IEnumerable<ApproachDelayResult>>
    {
        //protected JoinBlock<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>> mergeCalculateDelayValues;

        //internal GetDetectorEvents GetDetectorEvents { get; private set; }

        //public FilteredPhaseIntervalChanges FilteredPhaseIntervalChanges { get; private set; }

        //public FilteredDetectorData FilteredDetectorData { get; private set; }
        //public CreateRedToRedCycles CreateRedToRedCycles { get; private set; }
        //public IdentifyandAdjustVehicleActivations IdentifyandAdjustVehicleActivations { get; private set; }
        //public CalculateApproachDelay CalculateDelayValues { get; private set; }
        //public GenerateApproachDelayResults GenerateApproachDelayResults { get; private set; }

        public override void InstantiateSteps()
        {
            //FilteredPhaseIntervalChanges = new();
            //FilteredDetectorData = new();
            //CreateRedToRedCycles = new();
            //IdentifyandAdjustVehicleActivations = new();
            //mergeCalculateDelayValues = new();
            //CalculateDelayValues = new();
            //GenerateApproachDelayResults = new();

            //GetDetectorEvents = new();
        }

        public override void AddStepsToTracker()
        {
            //Steps.Add(FilteredPhaseIntervalChanges);
            //Steps.Add(FilteredDetectorData);
            //Steps.Add(CreateRedToRedCycles);
            //Steps.Add(IdentifyandAdjustVehicleActivations);
            //Steps.Add(mergeCalculateDelayValues);
            //Steps.Add(CalculateDelayValues);
            //Steps.Add(GenerateApproachDelayResults);

            //Steps.Add(GetDetectorEvents);
        }

        public override void LinkSteps()
        {
            //Input.LinkTo(FilteredPhaseIntervalChanges, new DataflowLinkOptions() { PropagateCompletion = true });
            //Input.LinkTo(FilteredDetectorData, new DataflowLinkOptions() { PropagateCompletion = true });

            //FilteredPhaseIntervalChanges.LinkTo(CreateRedToRedCycles, new DataflowLinkOptions() { PropagateCompletion = true });
            //FilteredDetectorData.LinkTo(GetDetectorEvents, new DataflowLinkOptions() { PropagateCompletion = true });
            //GetDetectorEvents.LinkTo(IdentifyandAdjustVehicleActivations, new DataflowLinkOptions() { PropagateCompletion = true });
            //IdentifyandAdjustVehicleActivations.LinkTo(mergeCalculateDelayValues.Target1, new DataflowLinkOptions() { PropagateCompletion = true });
            //CreateRedToRedCycles.LinkTo(mergeCalculateDelayValues.Target2, new DataflowLinkOptions() { PropagateCompletion = true });
            //mergeCalculateDelayValues.LinkTo(CalculateDelayValues, new DataflowLinkOptions() { PropagateCompletion = true });
            //CalculateDelayValues.LinkTo(GenerateApproachDelayResults, new DataflowLinkOptions() { PropagateCompletion = true });
            //GenerateApproachDelayResults.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }

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
                        LatencyCorrection = 1.2,
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
