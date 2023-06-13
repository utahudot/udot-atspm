using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.Plans;
using ATSPM.Application.Analysis.PurdueCoordination;
using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.BaseClasses;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.ApproachDelay
{
    public class ApproachDelayWorkflow : WorkflowBase<IEnumerable<ControllerEventLog>, ApproachDelayResult>
    {
        private readonly DataflowBlockOptions _filterOptions = new DataflowBlockOptions();
        private readonly ExecutionDataflowBlockOptions _stepOptions = new ExecutionDataflowBlockOptions();
        
        public ApproachDelayWorkflow(int maxDegreeOfParallelism = 1, CancellationToken cancellationToken = default)
        {
            _filterOptions.CancellationToken = cancellationToken;
            _stepOptions.CancellationToken = cancellationToken;
            _stepOptions.MaxDegreeOfParallelism = maxDegreeOfParallelism;
        }
        
        protected JoinBlock<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>> mergeCyclesAndVehicles;
        //protected JoinBlock<IReadOnlyList<Vehicle>, IReadOnlyList<ApproachDelayPlan>> mergePlansAndDelayResults;

        internal GetDetectorEvents GetDetectorEvents { get; private set; }

        //public FilteredPlanData FilteredPlanData { get; private set; }
        public FilteredPhaseIntervalChanges FilteredPhaseIntervalChanges { get; private set; }
        public FilteredDetectorData FilteredDetectorData { get; private set; }
        //public CalculateTimingPlans<ApproachDelayPlan> CalculateTimingPlans { get; private set; }
        public CreateRedToRedCycles CreateRedToRedCycles { get; private set; }
        public IdentifyandAdjustVehicleActivations IdentifyandAdjustVehicleActivations { get; private set; }
        public AssignCyclesToVehicles AssignCyclesToVehicles { get; private set; }
        //public AssignRangeToPlan<ApproachDelayPlan> AssignRangeToPlan { get; private set; }
        public GenerateApproachDelayResults GenerateApproachDelayResults { get; private set; }

        public override void InstantiateSteps()
        {
            //FilteredPlanData = new();
            FilteredPhaseIntervalChanges = new();
            FilteredDetectorData = new();
            //CalculateTimingPlans = new();
            CreateRedToRedCycles = new();
            IdentifyandAdjustVehicleActivations = new();
            mergeCyclesAndVehicles = new();
            AssignCyclesToVehicles = new();
            //mergePlansAndDelayResults = new();
            //AssignRangeToPlan = new();
            GenerateApproachDelayResults = new();

            GetDetectorEvents = new();
        }

        public override void AddStepsToTracker()
        {
            //Steps.Add(FilteredPlanData);
            Steps.Add(FilteredPhaseIntervalChanges);
            Steps.Add(FilteredDetectorData);
            //Steps.Add(CalculateTimingPlans);
            Steps.Add(CreateRedToRedCycles);
            Steps.Add(IdentifyandAdjustVehicleActivations);
            Steps.Add(mergeCyclesAndVehicles);
            Steps.Add(AssignCyclesToVehicles);
            //Steps.Add(mergePlansAndDelayResults);
            //Steps.Add(AssignRangeToPlan);
            Steps.Add(GenerateApproachDelayResults);

            Steps.Add(GetDetectorEvents);
        }

        public override void LinkSteps()
        {
            //Input.LinkTo(FilteredPlanData, new DataflowLinkOptions() { PropagateCompletion = true });
            Input.LinkTo(FilteredPhaseIntervalChanges, new DataflowLinkOptions() { PropagateCompletion = true });
            Input.LinkTo(FilteredDetectorData, new DataflowLinkOptions() { PropagateCompletion = true });

            //FilteredPlanData.LinkTo(CalculateTimingPlans, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredPhaseIntervalChanges.LinkTo(CreateRedToRedCycles, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredDetectorData.LinkTo(GetDetectorEvents, new DataflowLinkOptions() { PropagateCompletion = true });
            GetDetectorEvents.LinkTo(IdentifyandAdjustVehicleActivations, new DataflowLinkOptions() { PropagateCompletion = true });
            IdentifyandAdjustVehicleActivations.LinkTo(mergeCyclesAndVehicles.Target1, new DataflowLinkOptions() { PropagateCompletion = true });
            CreateRedToRedCycles.LinkTo(mergeCyclesAndVehicles.Target2, new DataflowLinkOptions() { PropagateCompletion = true });
            mergeCyclesAndVehicles.LinkTo(AssignCyclesToVehicles, new DataflowLinkOptions() { PropagateCompletion = true });

            //AssignCyclesToVehicles.LinkTo(mergePlansAndDelayResults.Target1, new DataflowLinkOptions() { PropagateCompletion = true });
            //CalculateTimingPlans.LinkTo(mergePlansAndDelayResults.Target2, new DataflowLinkOptions() { PropagateCompletion = true });

            //mergePlansAndDelayResults.LinkTo(AssignRangeToPlan, new DataflowLinkOptions() { PropagateCompletion = true });
            //AssignRangeToPlan.LinkTo(GenerateApproachDelayResults, new DataflowLinkOptions() { PropagateCompletion = true });

            AssignCyclesToVehicles.LinkTo(GenerateApproachDelayResults, new DataflowLinkOptions() { PropagateCompletion = true });

            GenerateApproachDelayResults.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
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
