#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/FilterEventsByTypeStep.cs
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

namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
{
    /// <summary>
    /// A workflow step that filters <see cref="EventLogModelBase"/> instances by a specific event type.
    /// </summary>
    /// <typeparam name="T">
    /// The type of event log to filter for. Must inherit from <see cref="EventLogModelBase"/>.
    /// </typeparam>
    /// <remarks>
    /// This process step takes a tuple containing a <see cref="Location"/> and a collection of event logs.
    /// It applies the <see cref="EventLogSpecification"/> to ensure logs are valid for the given location,
    /// then filters the collection to only include events of type <typeparamref name="T"/>.
    /// The result is returned as a tuple of the location and the filtered events.
    /// </remarks>
    /// <remarks>
    /// Initializes a new instance of the <see cref="FilterEventsByTypeStep{T}"/> class
    /// with the specified dataflow block options.
    /// </remarks>
    /// <param name="dataflowBlockOptions">
    /// Options that configure the execution behavior of the dataflow block,
    /// such as cancellation and parallelism. Defaults to <c>null</c>.
    /// </param>
    public class FilterEventsByTypeStep<T>(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : TransformProcessStepBase<Tuple<Location, IEnumerable<EventLogModelBase>>, Tuple<Location, IEnumerable<T>>>(dataflowBlockOptions) where T : EventLogModelBase
    {

        /// <summary>
        /// Processes the input tuple by filtering event logs to only include those of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="input">
        /// A tuple containing the <see cref="Location"/> and a collection of <see cref="EventLogModelBase"/> instances.
        /// </param>
        /// <param name="cancelToken">
        /// A cancellation token used to cancel the operation if requested.
        /// </param>
        /// <returns>
        /// A task that produces a tuple containing the location and the filtered collection of events of type <typeparamref name="T"/>.
        /// </returns>
        /// <remarks>
        /// The filtering logic applies the <see cref="EventLogSpecification"/> for the given location
        /// and then restricts the results to events matching the specified type.
        /// </remarks>
        protected override Task<Tuple<Location, IEnumerable<T>>> Process(Tuple<Location, IEnumerable<EventLogModelBase>> input, CancellationToken cancelToken = default)
        {
            var location = input.Item1;
            var events = input.Item2
                .FromSpecification(new EventLogSpecification(location))
                .Where(w => w.GetType() == typeof(T))
                .Cast<T>()
                .ToList()
                .AsEnumerable();

            return Task.FromResult(Tuple.Create(location, events));
        }
    }
}
