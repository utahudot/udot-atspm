#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Configuration/SignalTimingPlanBackfillConfiguration.cs
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

using System.ComponentModel.DataAnnotations;

namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    /// <summary>
    /// Configuration options for backfilling signal timing plans from stored event logs.
    /// </summary>
    public class SignalTimingPlanBackfillConfiguration
    {
        /// <summary>
        /// Start of the plan backfill window.
        /// </summary>
        [Required]
        public DateTime Start { get; set; }

        /// <summary>
        /// End of the plan backfill window.
        /// </summary>
        [Required]
        public DateTime End { get; set; }

        /// <summary>
        /// Number of plan events to batch through the timing-plan workflow.
        /// </summary>
        [Range(1, 1000000, ErrorMessage = "It is not recommended to set Processing Batch Size greater than 1,000,000")]
        public int ProcessingBatchSize { get; set; } = 50000;

        /// <summary>
        /// The number of concurrent workflow operations.
        /// </summary>
        [Range(1, 100, ErrorMessage = "It is not recommended to set Parallel Processes than 100")]
        public int ParallelProcesses { get; set; } = 5;

        /// <summary>
        /// The pre/post query offset in hours used to capture boundary plan changes.
        /// </summary>
        [Range(0, 72, ErrorMessage = "The offset hours cannot exceed 72 hours")]
        public int SignalTimingPlanOffsetHours { get; set; } = 72;

        /// <inheritdoc cref="EventAggregationQueryOptions"/>
        public EventAggregationQueryOptions EventAggregationQueryOptions { get; set; } = new();
    }
}
