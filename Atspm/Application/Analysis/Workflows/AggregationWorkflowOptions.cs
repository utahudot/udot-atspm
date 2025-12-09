#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.Workflows/AggregationWorkflowOptions.cs
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

namespace Utah.Udot.Atspm.Analysis.Workflows
{
    /// <summary>
    /// Defines configuration options for an aggregation workflow.
    /// </summary>
    public class AggregationWorkflowOptions
    {
        /// <summary>
        /// Gets or sets the timeline range used to control the aggregation window.
        /// </summary>
        /// <remarks>
        /// The timeline specifies the start and end ranges over which aggregation
        /// operations will be performed.
        /// </remarks>
        public Timeline<StartEndRange> Timeline { get; set; }

        /// <summary>
        /// Gets or sets the maximum degree of parallelism allowed during workflow execution.
        /// </summary>
        /// <remarks>
        /// Defaults to 1, meaning sequential execution. Increasing this value allows
        /// multiple tasks to run concurrently.
        /// </remarks>
        public int MaxDegreeOfParallelism { get; set; } = 1;

        /// <summary>
        /// Gets or sets the cancellation token used to cancel workflow execution.
        /// </summary>
        /// <remarks>
        /// This token can be signaled to stop ongoing aggregation operations gracefully.
        /// </remarks>
        public CancellationToken CancellationToken { get; set; }
    }
}
