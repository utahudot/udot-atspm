using ATSPM.Application.Analysis.ApproachDelay;
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Data.Models;
using ATSPM.Domain.Workflows;
using System;
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
    public class ApproachDelayWorkflow : WorkflowBase<Tuple<Signal, IEnumerable<ControllerEventLog>>, Tuple<Approach, ApproachDelayResult>>
    {
        private readonly DataflowBlockOptions _filterOptions = new DataflowBlockOptions();
        private readonly ExecutionDataflowBlockOptions _stepOptions = new ExecutionDataflowBlockOptions();

        public ApproachDelayWorkflow(int maxDegreeOfParallelism = 1, CancellationToken cancellationToken = default)
        {
            _filterOptions.CancellationToken = cancellationToken;
            _stepOptions.CancellationToken = cancellationToken;
            _stepOptions.MaxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        protected JoinBlock<Tuple<Approach,IEnumerable<CorrectedDetectorEvent>>, Tuple<Approach, IEnumerable<RedToRedCycle>>> mergeCyclesAndVehicles;

        public FilteredPhaseIntervalChanges FilteredPhaseIntervalChanges { get; private set; }
        public FilteredDetectorData FilteredDetectorData { get; private set; }

        public GroupSignalsByApproaches GroupSignalsByApproaches1 { get; private set; }
        public GroupSignalsByApproaches GroupSignalsByApproaches2 { get; private set; }


        public CreateRedToRedCycles CreateRedToRedCycles { get; private set; }
        public IdentifyandAdjustVehicleActivations IdentifyandAdjustVehicleActivations { get; private set; }
        public CreateVehicle CreateVehicle { get; private set; }
        public GenerateApproachDelayResults GenerateApproachDelayResults { get; private set; }

        /// <inheritdoc/>
        public override void InstantiateSteps()
        {
            FilteredPhaseIntervalChanges = new();
            FilteredDetectorData = new();

            GroupSignalsByApproaches1 = new();
            GroupSignalsByApproaches2 = new();

            CreateRedToRedCycles = new();
            IdentifyandAdjustVehicleActivations = new();
            mergeCyclesAndVehicles = new();
            CreateVehicle = new();
            GenerateApproachDelayResults = new();
        }

        /// <inheritdoc/>
        public override void AddStepsToTracker()
        {
            Steps.Add(FilteredPhaseIntervalChanges);
            Steps.Add(FilteredDetectorData);

            Steps.Add(GroupSignalsByApproaches1);
            Steps.Add(GroupSignalsByApproaches2);

            Steps.Add(CreateRedToRedCycles);
            Steps.Add(IdentifyandAdjustVehicleActivations);
            Steps.Add(mergeCyclesAndVehicles);
            Steps.Add(CreateVehicle);
            Steps.Add(GenerateApproachDelayResults);
        }

        /// <inheritdoc/>
        public override void LinkSteps()
        {
            Input.LinkTo(FilteredPhaseIntervalChanges, new DataflowLinkOptions() { PropagateCompletion = true });
            Input.LinkTo(FilteredDetectorData, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredPhaseIntervalChanges.LinkTo(GroupSignalsByApproaches1, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredDetectorData.LinkTo(GroupSignalsByApproaches2, new DataflowLinkOptions() { PropagateCompletion = true });

            GroupSignalsByApproaches1.LinkTo(CreateRedToRedCycles, new DataflowLinkOptions() { PropagateCompletion = true });
            GroupSignalsByApproaches2.LinkTo(IdentifyandAdjustVehicleActivations, new DataflowLinkOptions() { PropagateCompletion = true });

            IdentifyandAdjustVehicleActivations.LinkTo(mergeCyclesAndVehicles.Target1, new DataflowLinkOptions() { PropagateCompletion = true });
            CreateRedToRedCycles.LinkTo(mergeCyclesAndVehicles.Target2, new DataflowLinkOptions() { PropagateCompletion = true });
            mergeCyclesAndVehicles.LinkTo(CreateVehicle, new DataflowLinkOptions() { PropagateCompletion = true });
            CreateVehicle.LinkTo(GenerateApproachDelayResults, new DataflowLinkOptions() { PropagateCompletion = true });
            GenerateApproachDelayResults.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }
}
