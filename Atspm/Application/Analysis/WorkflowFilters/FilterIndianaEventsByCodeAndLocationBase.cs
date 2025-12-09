#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowFilters/FilterIndianaEventsByCodeAndLocationBase.cs
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
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Extensions;
using Utah.Udot.NetStandardToolkit.Specifications;

namespace Utah.Udot.Atspm.Analysis.WorkflowFilters
{
    /// <summary>
    /// Provides a base implementation for filtering Indiana event log data 
    /// by event code and location within process workflows.
    /// </summary>
    /// <remarks>
    /// This abstract class defines a reusable workflow step that applies 
    /// filtering logic to <see cref="IndianaEvent"/> collections associated 
    /// with a given <see cref="Location"/>. 
    /// It leverages specifications to enforce filtering rules and broadcasts 
    /// the filtered results downstream in the workflow pipeline.
    /// </remarks>
    public abstract class FilterIndianaEventsByCodeAndLocationBase
        : ProcessStepBase<Tuple<Location, IEnumerable<IndianaEvent>>, Tuple<Location, IEnumerable<IndianaEvent>>>
    {
        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="FilterIndianaEventsByCodeAndLocationBase"/> class.
        /// </summary>
        /// <param name="specification">
        /// The specification used to filter <see cref="IndianaEvent"/> objects 
        /// based on custom business rules (e.g., event codes).
        /// </param>
        /// <param name="dataflowBlockOptions">
        /// Optional configuration settings for the underlying dataflow block, 
        /// such as degree of parallelism or cancellation tokens. 
        /// Defaults to <c>null</c> if not provided.
        /// </param>
        /// <remarks>
        /// The constructor sets up a <see cref="BroadcastBlock{T}"/> that 
        /// applies both a location-based filter 
        /// (<see cref="EventLogSpecification"/>) and the 
        /// provided specification to the incoming event stream. 
        /// Completion of the workflow process is logged to the console.
        /// </remarks>
        public FilterIndianaEventsByCodeAndLocationBase(ISpecification<IndianaEvent> specification, DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            workflowProcess = new BroadcastBlock<Tuple<Location, IEnumerable<IndianaEvent>>>(f =>
            {
                return Tuple.Create(
                    f.Item1,
                    f.Item2
                     .FromSpecification(new EventLogSpecification(f.Item1))
                     .Cast<IndianaEvent>()
                     .FromSpecification(specification));
            }, options);

            workflowProcess.Completion.ContinueWith(t => Console.WriteLine($"!!!Task {options.NameFormat} is complete!!!"));
        }
    }
}
