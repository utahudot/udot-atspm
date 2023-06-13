using ATSPM.Application.Analysis.ApproachVolume;
using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.Workflows
{
    /// <summary>
    /// Volume data can be useful when programming signal timing values or 
    /// troubleshooting detection issues and is also often collected for planning
    /// purposes.This measure reports the number of vehicles observed on an approach
    ///(1). The number of vehicles is normalized to a flow rate(in vehicles per hour).
    ///The data may be aggregated into custom-sized bins, with 15 minutes being the default.
    /// </summary>
    public class ApproachVolumeWorkflow : WorkflowBase<IEnumerable<ControllerEventLog>, ApproachVolumeResult>
    {
        //protected JoinBlock<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>> mergeCalculateDelayValues;

        protected GetDetectorEvents GetDetectorEvents { get; private set; }

        public FilteredDetectorData FilteredDetectorData { get; private set; }
        public IdentifyandAdjustVehicleActivations IdentifyandAdjustVehicleActivations { get; private set; }
        public CalculateTotalVolumes CalculateTotalVolumes { get; private set; }
        public GenerateApproachVolumeResults GenerateApproachVolumeResults { get; private set; }

        /// <inheritdoc/>
        public override void InstantiateSteps()
        {
            FilteredDetectorData = new();
            IdentifyandAdjustVehicleActivations = new();
            //HACK: figure this out!
            CalculateTotalVolumes = new(new TimelineOptions()
            {
                Start = DateTime.Parse("4/17/2023 8:00:0.0"),
                End = DateTime.Parse("4/17/2023 10:00:0.0"),
                Size = 15
            });
            GenerateApproachVolumeResults = new();

            GetDetectorEvents = new();
        }

        /// <inheritdoc/>
        public override void AddStepsToTracker()
        {
            Steps.Add(FilteredDetectorData);
            Steps.Add(IdentifyandAdjustVehicleActivations);
            Steps.Add(CalculateTotalVolumes);
            Steps.Add(GenerateApproachVolumeResults);

            Steps.Add(GetDetectorEvents);
        }

        /// <inheritdoc/>
        public override void LinkSteps()
        {
            Input.LinkTo(FilteredDetectorData, new DataflowLinkOptions() { PropagateCompletion = true });

            FilteredDetectorData.LinkTo(GetDetectorEvents, new DataflowLinkOptions() { PropagateCompletion = true });
            GetDetectorEvents.LinkTo(IdentifyandAdjustVehicleActivations, new DataflowLinkOptions() { PropagateCompletion = true });
            IdentifyandAdjustVehicleActivations.LinkTo(CalculateTotalVolumes, new DataflowLinkOptions() { PropagateCompletion = true });
            CalculateTotalVolumes.LinkTo(GenerateApproachVolumeResults, new DataflowLinkOptions() { PropagateCompletion = true });
            GenerateApproachVolumeResults.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }
}
