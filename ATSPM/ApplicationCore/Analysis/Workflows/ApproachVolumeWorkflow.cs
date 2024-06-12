#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - %Namespace%/ApproachVolumeWorkflow.cs
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
    /// <summary>
    /// Volume data can be useful when programming Location timing values or 
    /// troubleshooting detection issues and is also often collected for planning
    /// purposes.This measure reports the number of vehicles observed on an approach
    ///(1). The number of vehicles is normalized to a flow rate(in vehicles per hour).
    ///The data may be aggregated into custom-sized bins, with 15 minutes being the default.
    /// </summary>
    public class ApproachVolumeWorkflow : WorkflowBase<Tuple<Approach, IEnumerable<ControllerEventLog>>, ApproachVolumeResult>
    {
        protected JoinBlock<IEnumerable<Tuple<Detector, IEnumerable<CorrectedDetectorEvent>>>, Tuple<Approach, IEnumerable<RedToRedCycle>>> mergeCyclesAndVehicles;

        public FilteredDetectorData FilteredDetectorData { get; private set; }
        public IdentifyandAdjustVehicleActivations IdentifyandAdjustVehicleActivations { get; private set; }
        public CalculateTotalVolumes CalculateTotalVolumes { get; private set; }
        public GenerateApproachVolumeResults GenerateApproachVolumeResults { get; private set; }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            FilteredDetectorData = new();
            IdentifyandAdjustVehicleActivations = new();
            CalculateTotalVolumes = new();

            GenerateApproachVolumeResults = new();
        }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(FilteredDetectorData);
            Steps.Add(IdentifyandAdjustVehicleActivations);
            Steps.Add(CalculateTotalVolumes);

            Steps.Add(GenerateApproachVolumeResults);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(FilteredDetectorData, new DataflowLinkOptions() { PropagateCompletion = true });

            FilteredDetectorData.LinkTo(IdentifyandAdjustVehicleActivations, new DataflowLinkOptions() { PropagateCompletion = true });
            IdentifyandAdjustVehicleActivations.LinkTo(mergeCyclesAndVehicles.Target1, new DataflowLinkOptions() { PropagateCompletion = true });




            CalculateTotalVolumes.LinkTo(GenerateApproachVolumeResults, new DataflowLinkOptions() { PropagateCompletion = true });
            GenerateApproachVolumeResults.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }
}
