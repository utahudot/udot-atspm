#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Configuration/DeviceEventLoggingConfiguration.cs
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
    /// Configuration options for the Device Event Logging background service.
    /// </summary>
    public class DeviceEventLoggingConfiguration
    {
        /// <summary>
        /// The local directory path where event logs are temporarily stored or archived.
        /// </summary>
        public string Path { get; set; } = System.IO.Path.GetTempPath();

        /// <summary>
        /// The number of processed events to accumulate before performing a bulk database upsert.
        /// </summary>
        /// <remarks>
        /// Potato Analogy: This is the size of the **basket** at the end of the table. 
        /// Once the basket hits this limit, it is carried to the cellar (Database) to be stored.
        /// </remarks>
        public int ProcessingBatchSize { get; set; } = 50000;

        /// <summary>
        /// The number of concurrent threads processing items within a single workflow instance.
        /// </summary>
        /// <remarks>
        /// Potato Analogy: This is the number of **people peeling potatoes** at a single table. 
        /// More people peel the table's pile faster, but too many may bump elbows (CPU contention).
        /// </remarks>
        public int ParallelProcesses { get; set; } = 5;

        /// <summary>
        /// The maximum number of workflow instances to run concurrently.
        /// </summary>
        /// <remarks>
        /// Potato Analogy: This is the total number of **tables** set up in the kitchen. 
        /// Each table operates independently with its own set of peelers.
        /// </remarks>
        public int WorkflowBatchSize { get; set; } = 20;

        /// <summary>
        /// The number of devices assigned to a single workflow instance. 
        /// If null or 0, the system automatically balances the total device count across the available <see cref="WorkflowBatchSize"/>.
        /// </summary>
        /// <remarks>
        /// Potato Analogy: This is the size of the **pile of potatoes** delivered to each table.
        /// </remarks>
        public int? DevicesBatchSize { get; set; }

        /// <summary>
        /// The time window (in hours) used to buffer event queries, ensuring overlapping plans are captured for comparison.
        /// </summary>
        [Range(0, 72, ErrorMessage = "The offset hours cannot exceed 72 hours.")]
        public int SignalTimingPlanOffsetHours { get; set; } = 12;

        /// <inheritdoc cref="DeviceEventLoggingQueryOptions"/>
        public DeviceEventLoggingQueryOptions DeviceEventLoggingQueryOptions { get; set; } = new();

        public override string ToString()
        {
            return $"Path: {Path}, ProcessingBatchSize: {ProcessingBatchSize}, ParallelProcesses: {ParallelProcesses}, WorkflowBatchSize: {WorkflowBatchSize}, DevicesBatchSize: {DevicesBatchSize}, SignalTimingPlanOffsetHours: {SignalTimingPlanOffsetHours}";
        }
    }
}
