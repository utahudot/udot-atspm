#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.Workflows/AggregationWorkflowBase.cs
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
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Analysis.Workflows
{
    /// <summary>
    /// Provides a base class for aggregation workflows that process event logs
    /// into aggregation models.
    /// </summary>
    /// <typeparam name="T">
    /// The type of aggregation model produced by the workflow. Must inherit from <see cref="AggregationModelBase"/>.
    /// </typeparam>
    /// <remarks>
    /// This workflow base class uses <see cref="AggregationWorkflowOptions"/> to configure
    /// execution behavior such as parallelism and cancellation. It inherits from
    /// <see cref="WorkflowBase{TInput, TOutput}"/> where the input is a tuple of
    /// <see cref="Location"/> and a collection of <see cref="EventLogModelBase"/>,
    /// and the output is a collection of <typeparamref name="T"/>.
    /// </remarks>
    public abstract class AggregationWorkflowBase<T> : WorkflowBase<Tuple<Location, IEnumerable<EventLogModelBase>>, IEnumerable<T>> where T : AggregationModelBase
    {
        /// <summary>
        /// The workflow options that configure execution behavior such as timeline,
        /// parallelism, and cancellation.
        /// </summary>
        protected AggregationWorkflowOptions workflowOptions;

        /// <summary>
        /// The execution options applied to dataflow blocks within the workflow.
        /// </summary>
        protected ExecutionDataflowBlockOptions executionBlockOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregationWorkflowBase{T}"/> class
        /// with the specified workflow options.
        /// </summary>
        /// <param name="options">
        /// The workflow options used to configure execution. If not provided, defaults are used.
        /// </param>
        /// <remarks>
        /// The constructor sets up cancellation and parallelism options for dataflow execution
        /// based on the provided <paramref name="options"/>.
        /// </remarks>
        public AggregationWorkflowBase(AggregationWorkflowOptions options = default) : base(new DataflowBlockOptions() { CancellationToken = options.CancellationToken })
        {
            workflowOptions = options;
            executionBlockOptions = new ExecutionDataflowBlockOptions()
            {
                CancellationToken = options.CancellationToken,
                MaxDegreeOfParallelism = options.MaxDegreeOfParallelism,
            };
        }
    }
}
