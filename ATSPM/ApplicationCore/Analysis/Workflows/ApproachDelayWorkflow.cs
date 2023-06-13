using ATSPM.Application.Analysis.ApproachDelay;
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Data.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.Workflows
{
    /// <summary>
    /// Vehicle delay is a measure that is commonly modeled by agencies to identify 
    /// whether intersection operations are acceptable.Using high-resolution data, this
    /// measure can be computed directly.For locations with high delay, particularly at
    /// uncongested locations, signal timing adjustments can help reduce wait times (1).
    /// Approach delay is a measure that integrates individual vehicle delay with
    /// volume to get an estimated sum of all vehicle delay on an approach.
    /// </summary>
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
        protected GetDetectorEvents GetDetectorEvents { get; private set; }

        public FilteredPhaseIntervalChanges FilteredPhaseIntervalChanges { get; private set; }
        public FilteredDetectorData FilteredDetectorData { get; private set; }
        public CreateRedToRedCycles CreateRedToRedCycles { get; private set; }
        public IdentifyandAdjustVehicleActivations IdentifyandAdjustVehicleActivations { get; private set; }
        public AssignCyclesToVehicles AssignCyclesToVehicles { get; private set; }
        public GenerateApproachDelayResults GenerateApproachDelayResults { get; private set; }

        /// <inheritdoc/>
        public override void InstantiateSteps()
        {
            FilteredPhaseIntervalChanges = new();
            FilteredDetectorData = new();
            CreateRedToRedCycles = new();
            IdentifyandAdjustVehicleActivations = new();
            mergeCyclesAndVehicles = new();
            AssignCyclesToVehicles = new();
            GenerateApproachDelayResults = new();

            GetDetectorEvents = new();
        }

        /// <inheritdoc/>
        public override void AddStepsToTracker()
        {
            Steps.Add(FilteredPhaseIntervalChanges);
            Steps.Add(FilteredDetectorData);
            Steps.Add(CreateRedToRedCycles);
            Steps.Add(IdentifyandAdjustVehicleActivations);
            Steps.Add(mergeCyclesAndVehicles);
            Steps.Add(AssignCyclesToVehicles);
            Steps.Add(GenerateApproachDelayResults);

            Steps.Add(GetDetectorEvents);
        }

        /// <inheritdoc/>
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
            AssignCyclesToVehicles.LinkTo(GenerateApproachDelayResults, new DataflowLinkOptions() { PropagateCompletion = true });
            GenerateApproachDelayResults.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }
}
