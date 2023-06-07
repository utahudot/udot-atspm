using ATSPM.Application.Analysis.ApproachVolume;
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.Plans;
using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.PurdueCoordination
{
    public class GeneratePurdueCoordinationResult : TransformProcessStepBase<IReadOnlyList<PurdueCoordinationPlan>, PurdueCoordinationResult>
    {
        public GeneratePurdueCoordinationResult(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<PurdueCoordinationResult> Process(IReadOnlyList<PurdueCoordinationPlan> input, CancellationToken cancelToken = default)
        {
            var result = new PurdueCoordinationResult() { Plans = input };

            return Task.FromResult(result);
        }
    }

    public class PurdueCoordinationWorkflow : WorkflowBase<IEnumerable<ControllerEventLog>, PurdueCoordinationResult>
    {
        protected JoinBlock<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>> mergeVehicleArrivals;
        protected JoinBlock<IReadOnlyList<PurdueCoordinationPlan>, IReadOnlyList<IStartEndRange>> mergePlansAndCycles;

        internal GetDetectorEvents GetDetectorEvents { get; private set; }

        public FilteredPlanData FilteredPlanData { get; private set; }
        public FilteredPhaseIntervalChanges FilteredPhaseIntervalChanges { get; private set; }

        public FilteredDetectorData FilteredDetectorData { get; private set; }
        public CalculateTimingPlans<PurdueCoordinationPlan> CalculateTimingPlans { get; private set; }
        public CreateRedToRedCycles CreateRedToRedCycles { get; private set; }
        public IdentifyandAdjustVehicleActivations IdentifyandAdjustVehicleActivations { get; private set; }
        public CalculateVehicleArrivals CalculateVehicleArrivals { get; private set; }
        public AssignRangeToPlan<PurdueCoordinationPlan> AssignRangeToPlan { get; private set; }
        public GeneratePurdueCoordinationResult GeneratePurdueCoordinationResult { get; private set; }

        public override void InstantiateSteps()
        {
            FilteredPlanData = new();
            FilteredPhaseIntervalChanges = new();
            FilteredDetectorData = new();
            CalculateTimingPlans = new();
            CreateRedToRedCycles = new();
            IdentifyandAdjustVehicleActivations = new();
            mergeVehicleArrivals = new();
            CalculateVehicleArrivals = new();
            mergePlansAndCycles = new();
            AssignRangeToPlan = new();
            GeneratePurdueCoordinationResult = new();

            GetDetectorEvents = new();
        }

        public override void AddStepsToTracker()
        {
            Steps.Add(FilteredPlanData);
            Steps.Add(FilteredPhaseIntervalChanges);
            Steps.Add(FilteredDetectorData);
            Steps.Add(CalculateTimingPlans);
            Steps.Add(CreateRedToRedCycles);
            Steps.Add(IdentifyandAdjustVehicleActivations);
            Steps.Add(mergeVehicleArrivals);
            Steps.Add(CalculateVehicleArrivals);
            Steps.Add(mergePlansAndCycles);
            Steps.Add(AssignRangeToPlan);
            Steps.Add(GeneratePurdueCoordinationResult);

            Steps.Add(GetDetectorEvents);
        }

        public override void LinkSteps()
        {
            Input.LinkTo(FilteredPlanData, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredPlanData.LinkTo(CalculateTimingPlans, new DataflowLinkOptions() { PropagateCompletion = true });

            Input.LinkTo(FilteredPhaseIntervalChanges, new DataflowLinkOptions() { PropagateCompletion = true });
            Input.LinkTo(FilteredDetectorData, new DataflowLinkOptions() { PropagateCompletion = true });

            FilteredPhaseIntervalChanges.LinkTo(CreateRedToRedCycles, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredDetectorData.LinkTo(GetDetectorEvents, new DataflowLinkOptions() { PropagateCompletion = true });
            GetDetectorEvents.LinkTo(IdentifyandAdjustVehicleActivations, new DataflowLinkOptions() { PropagateCompletion = true });
            IdentifyandAdjustVehicleActivations.LinkTo(mergeVehicleArrivals.Target1, new DataflowLinkOptions() { PropagateCompletion = true });
            CreateRedToRedCycles.LinkTo(mergeVehicleArrivals.Target2, new DataflowLinkOptions() { PropagateCompletion = true });
            mergeVehicleArrivals.LinkTo(CalculateVehicleArrivals, new DataflowLinkOptions() { PropagateCompletion = true });
            CalculateTimingPlans.LinkTo(mergePlansAndCycles.Target1, new DataflowLinkOptions() { PropagateCompletion = true });
            CalculateVehicleArrivals.LinkTo(mergePlansAndCycles.Target2, new DataflowLinkOptions() { PropagateCompletion = true });
            mergePlansAndCycles.LinkTo(AssignRangeToPlan, new DataflowLinkOptions() { PropagateCompletion = true });
            AssignRangeToPlan.LinkTo(GeneratePurdueCoordinationResult, new DataflowLinkOptions() { PropagateCompletion = true });
            GeneratePurdueCoordinationResult.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
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
