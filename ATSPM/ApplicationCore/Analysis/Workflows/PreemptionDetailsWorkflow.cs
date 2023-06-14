using ATSPM.Application.Analysis.PreemptionDetails;
using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Data.Models;
using ATSPM.Domain.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.Workflows
{
    /// <summary>
    /// Preemption is the interruption of normal operations to serve a preferred 
    /// vehicle(e.g., train, emergency vehicle). This measure can be used to determine if 
    /// preemption events are occurring as intended. “Preemption Details” refers to the
    /// duration of preemption intervals for each preemption event. The type of interval
    /// information available depends on the type of preempt (i.e., rail or emergency
    /// vehicle) and the availability of certain inputs. Some potential intervals that can be
    /// tracked include entry delay, track clearance, gate down, dwell, time to service, 
    /// max-out, and preempt input on/off (1).
    /// </summary>
    public class PreemptionDetailsWorkflow : WorkflowBase<IEnumerable<ControllerEventLog>, PreemptDetailResult>
    {
        protected JoinBlock<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>> joinOne;
        protected JoinBlock<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>> joinTwo;
        protected JoinBlock<Tuple<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>, Tuple<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>> joinThree;
        internal MergePreemptionTimes mergePreemptionTimes;

        public FilteredPreemptionData FilteredPreemptionData { get; private set; }
        public CalculateDwellTime CalculateDwellTime { get; private set; }
        public CalculateTrackClearTime CalculateTrackClearTime { get; private set; }
        public CalculateTimeToService CalculateTimeToService { get; private set; }
        public CalculateDelay CalculateDelay { get; private set; }
        public CalculateTimeToGateDown CalculateTimeToGateDown { get; private set; }
        public CalculateTimeToCallMaxOut CalculateTimeToCallMaxOut { get; private set; }
        public GeneratePreemptDetailResults GeneratePreemptDetailResults { get; private set; }

        /// <inheritdoc/>
        public override void InstantiateSteps()
        {
            FilteredPreemptionData = new();

            CalculateDwellTime = new();
            CalculateTrackClearTime = new();
            CalculateTimeToService = new();
            CalculateDelay = new();
            CalculateTimeToGateDown = new();
            CalculateTimeToCallMaxOut = new();

            GeneratePreemptDetailResults = new();

            joinOne = new();
            joinTwo = new();
            joinThree = new();
            mergePreemptionTimes = new();
        }

        /// <inheritdoc/>
        public override void AddStepsToTracker()
        {
            Steps.Add(FilteredPreemptionData);
            Steps.Add(CalculateDwellTime);
            Steps.Add(CalculateTrackClearTime);
            Steps.Add(CalculateTimeToService);
            Steps.Add(CalculateDelay);
            Steps.Add(CalculateTimeToGateDown);
            Steps.Add(CalculateTimeToCallMaxOut);
            Steps.Add(GeneratePreemptDetailResults);
            Steps.Add(joinOne);
            Steps.Add(joinTwo);
            Steps.Add(joinThree);
            Steps.Add(mergePreemptionTimes);
        }

        /// <inheritdoc/>
        public override void LinkSteps()
        {
            Input.LinkTo(FilteredPreemptionData, new DataflowLinkOptions() { PropagateCompletion = true });

            FilteredPreemptionData.LinkTo(CalculateDwellTime, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredPreemptionData.LinkTo(CalculateTrackClearTime, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredPreemptionData.LinkTo(CalculateTimeToService, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredPreemptionData.LinkTo(CalculateDelay, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredPreemptionData.LinkTo(CalculateTimeToGateDown, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredPreemptionData.LinkTo(CalculateTimeToCallMaxOut, new DataflowLinkOptions() { PropagateCompletion = true });

            joinOne.LinkTo(joinThree.Target1, new DataflowLinkOptions() { PropagateCompletion = true });
            joinTwo.LinkTo(joinThree.Target2, new DataflowLinkOptions() { PropagateCompletion = true });

            CalculateDwellTime.LinkTo(joinOne.Target1, new DataflowLinkOptions() { PropagateCompletion = true });
            CalculateTrackClearTime.LinkTo(joinOne.Target2, new DataflowLinkOptions() { PropagateCompletion = true });
            CalculateTimeToService.LinkTo(joinOne.Target3, new DataflowLinkOptions() { PropagateCompletion = true });
            CalculateDelay.LinkTo(joinTwo.Target1, new DataflowLinkOptions() { PropagateCompletion = true });
            CalculateTimeToGateDown.LinkTo(joinTwo.Target2, new DataflowLinkOptions() { PropagateCompletion = true });
            CalculateTimeToCallMaxOut.LinkTo(joinTwo.Target3, new DataflowLinkOptions() { PropagateCompletion = true });

            joinThree.LinkTo(mergePreemptionTimes, new DataflowLinkOptions() { PropagateCompletion = true });
            mergePreemptionTimes.LinkTo(GeneratePreemptDetailResults, new DataflowLinkOptions() { PropagateCompletion = true });
            GeneratePreemptDetailResults.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }

    internal class MergePreemptionTimes : TransformProcessStepBase<Tuple<Tuple<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>, Tuple<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>>, IEnumerable<PreempDetailValueBase>>
    {
        internal MergePreemptionTimes(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<PreempDetailValueBase>> Process(Tuple<Tuple<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>, Tuple<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>> input, CancellationToken cancelToken = default)
        {
            var result = input.Item1.Item1.Union(input.Item1.Item2.Union(input.Item1.Item3)).Union(input.Item2.Item1.Union(input.Item2.Item2.Union(input.Item2.Item3)));

            return Task.FromResult(result);
        }
    }
}
