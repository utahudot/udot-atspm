﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - %Namespace%/PurdueCoordinationWorkflow.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Utah.Udot.Atspm.Analysis.Common;
using Utah.Udot.Atspm.Analysis.PurdueCoordination;
using Utah.Udot.Atspm.Analysis.WorkflowFilters;
using Utah.Udot.Atspm.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.Workflows
{
    /// <summary>
    /// The Purdue Coordination Diagram provides additional details on vehicle 
    /// arrivals during the cycle(e.g., during the green phase or red phase for each
    /// approach). The percent arrival on green, platoon ratio, and arrival type can help
    /// identify locations that would benefit from adjustments to Location timing(i.e., to
    /// cycle lengths, splits, offsets, and phase order), and the Purdue Coordination
    /// Diagram can help identify the values that should be chosen for those
    /// adjustments.It can also be used to monitor general intersection operations.With
    /// the cycle length being displayed in the diagram, this measure can be useful for 
    /// monitoring advanced applications such as traffic responsive or adaptive control
    /// (1). For Purdue Coordination Diagrams ATSPM defines the cycle from the
    /// beginning of the red interval to the beginning of the next red interval for the
    /// same phase.This measure can only be generated for approaches with advance
    /// detection.
    /// </summary>
    public class PurdueCoordinationWorkflow : WorkflowBase<IEnumerable<ControllerEventLog>, PurdueCoordinationResult>
    {
        protected JoinBlock<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>> mergeVehicleArrivals;
        
        protected GetDetectorEvents GetDetectorEvents { get; private set; }

        public FilteredPhaseIntervalChanges FilteredPhaseIntervalChanges { get; private set; }
        public FilteredDetectorData FilteredDetectorData { get; private set; }
        public CreateRedToRedCycles CreateRedToRedCycles { get; private set; }
        public IdentifyandAdjustVehicleActivations IdentifyandAdjustVehicleActivations { get; private set; }
        public CalculateVehicleArrivals CalculateVehicleArrivals { get; private set; }
        public GeneratePurdueCoordinationResult GeneratePurdueCoordinationResult { get; private set; }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            FilteredPhaseIntervalChanges = new();
            FilteredDetectorData = new();
            CreateRedToRedCycles = new();
            IdentifyandAdjustVehicleActivations = new();
            mergeVehicleArrivals = new();
            CalculateVehicleArrivals = new();
            GeneratePurdueCoordinationResult = new();

            GetDetectorEvents = new();
        }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(FilteredPhaseIntervalChanges);
            Steps.Add(FilteredDetectorData);
            Steps.Add(CreateRedToRedCycles);
            Steps.Add(IdentifyandAdjustVehicleActivations);
            Steps.Add(mergeVehicleArrivals);
            Steps.Add(CalculateVehicleArrivals);
            Steps.Add(GeneratePurdueCoordinationResult);

            Steps.Add(GetDetectorEvents);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(FilteredPhaseIntervalChanges, new DataflowLinkOptions() { PropagateCompletion = true });
            Input.LinkTo(FilteredDetectorData, new DataflowLinkOptions() { PropagateCompletion = true });

            FilteredPhaseIntervalChanges.LinkTo(CreateRedToRedCycles, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredDetectorData.LinkTo(GetDetectorEvents, new DataflowLinkOptions() { PropagateCompletion = true });
            GetDetectorEvents.LinkTo(IdentifyandAdjustVehicleActivations, new DataflowLinkOptions() { PropagateCompletion = true });
            IdentifyandAdjustVehicleActivations.LinkTo(mergeVehicleArrivals.Target1, new DataflowLinkOptions() { PropagateCompletion = true });
            CreateRedToRedCycles.LinkTo(mergeVehicleArrivals.Target2, new DataflowLinkOptions() { PropagateCompletion = true });
            mergeVehicleArrivals.LinkTo(CalculateVehicleArrivals, new DataflowLinkOptions() { PropagateCompletion = true });
            CalculateVehicleArrivals.LinkTo(GeneratePurdueCoordinationResult, new DataflowLinkOptions() { PropagateCompletion = true });
            GeneratePurdueCoordinationResult.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }
}
