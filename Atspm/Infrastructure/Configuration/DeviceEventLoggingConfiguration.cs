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

using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    /// <summary>
    /// Configuration options for device event logging
    /// </summary>
    public class DeviceEventLoggingConfiguration
    {
        /// <summary>
        /// Path to local directory where event logs are saved
        /// </summary>
        public string Path { get; set; } = System.IO.Path.GetTempPath();

        /// <summary>
        /// Batch size of <see cref="EventLogModelBase"/> chunks to to upsert to the repository at a time.
        /// </summary>
        public int ProcessingBatchSize { get; set; } = 50000;

        /// <summary>
        /// The amount of parallel process to run in the workflow
        /// </summary>
        public int ParallelProcesses { get; set; } = 5;

        public int WorkflowBatchSize { get; set; } = 20;

        public int DevicesBatchSize { get; set; }

        /// <summary>
        /// The number of hours to look behind and ahead when querying for events to log.
        /// This is to ensure the previous plan is pulled in so it can be merged and compared with the plans being logged.
        /// </summary>
        public int SignalTimingPlanOffsetHours { get; set; } = 12;

        /// <inheritdoc cref="DeviceEventLoggingQueryOptions"/>
        public DeviceEventLoggingQueryOptions DeviceEventLoggingQueryOptions { get; set; } = new();
    }
}
