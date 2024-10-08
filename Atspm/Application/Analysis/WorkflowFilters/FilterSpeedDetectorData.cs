﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.WorkflowFilters/FilterEventCodeSignalBase.cs
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

using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Analysis.WorkflowFilters
{
    /// <summary>
    /// Base class for filter controller event log data used in process workflows
    /// </summary>
    public abstract class FilterSpeedDetectorData : ProcessStepBase<Tuple<Location, IEnumerable<IndianaEvent>, IEnumerable<SpeedEvent>>, Tuple<Location, IEnumerable<IndianaEvent>, IEnumerable<SpeedEvent>>>
    {
        /// <summary>
        /// List of filtered event codes
        /// </summary>
        protected List<int> filteredList = new();

        /// <inheritdoc/>
        public FilterSpeedDetectorData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            workflowProcess = new BroadcastBlock<Tuple<Location, IEnumerable<IndianaEvent>, IEnumerable<SpeedEvent>>>(input =>
            {
                var location = input.Item1;
                var logs = input.Item2;
                var speedLogs = input.Item3;

                var filteredApproaches = location.Approaches
                        .Where(approach => approach.Detectors
                            .Any(detector => detector.DetectionTypes
                                .Any(detectionType => detectionType.Id == DetectionTypes.AC || detectionType.Id == DetectionTypes.AS)))
                        .ToList();
                location.Approaches = filteredApproaches;

                return Tuple.Create(location, logs, speedLogs);
            }, options);
            workflowProcess.Completion.ContinueWith(t => Console.WriteLine($"!!!Task {options.NameFormat} is complete!!!"));
        }
    }
}
