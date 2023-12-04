using ATSPM.Application.Analysis.ApproachVolume;
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.Workflows
{
    public class AggregateControllerDataWorkflow : WorkflowBase<>
    {
        public GroupSignalsByApproaches GroupSignalsByApproaches { get; private set; }
        public FilteredDetectorData FilteredDetectorData { get; private set; }
        public GroupDetectorsByDetectorEvent GroupDetectorsByDetectorEvent { get; private set; }
        public AggregateDetectorEvents AggregateDetectorEvents { get; private set; }

        /// <inheritdoc/>
        public override void InstantiateSteps()
        {
            GroupSignalsByApproaches = new();
            FilteredDetectorData = new();
            GroupDetectorsByDetectorEvent = new();
            AggregateDetectorEvents = new();
        }

        /// <inheritdoc/>
        public override void AddStepsToTracker()
        {
            Steps.Add(GroupSignalsByApproaches);
            Steps.Add(FilteredDetectorData);
            Steps.Add(GroupDetectorsByDetectorEvent);
            Steps.Add(AggregateDetectorEvents);
        }

        /// <inheritdoc/>
        public override void LinkSteps()
        {
            Input.LinkTo(GroupSignalsByApproaches, new DataflowLinkOptions() { PropagateCompletion = true });

            GroupSignalsByApproaches.LinkTo(filteredDetectorData, new DataflowLinkOptions() { PropagateCompletion = true });
            filteredDetectorData.LinkTo(FilteredDetectorData, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredDetectorData.LinkTo(GroupDetectorsByDetectorEvent, new DataflowLinkOptions() { PropagateCompletion = true });
            GroupDetectorsByDetectorEvent.LinkTo(AggregateDetectorEvents, new DataflowLinkOptions() { PropagateCompletion = true });

            AggregateDetectorEvents.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }
}
