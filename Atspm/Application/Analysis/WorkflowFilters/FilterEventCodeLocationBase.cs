#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowFilters/FilterEventCodeLocationBase.cs
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

namespace Utah.Udot.Atspm.Analysis.WorkflowFilters
{
    /// <summary>
    /// Base workflow filter for Indiana event-code filters that are also constrained to the input location.
    /// </summary>
    public abstract class FilterEventCodeLocationBase : ProcessStepBase<Tuple<Location, IEnumerable<IndianaEvent>>, Tuple<Location, IEnumerable<IndianaEvent>>>
    {
        /// <summary>
        /// Indiana event codes accepted by this filter.
        /// </summary>
        protected List<int> filteredList = new();

        /// <inheritdoc/>
        protected FilterEventCodeLocationBase(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            workflowProcess = new BroadcastBlock<Tuple<Location, IEnumerable<IndianaEvent>>>(f =>
            {
                return Tuple.Create(
                    f.Item1,
                    f.Item2
                        .FromSpecification(new EventLogSpecification(f.Item1))
                        .Cast<IndianaEvent>()
                        .Where(w => filteredList.Contains(w.EventCode)));
            }, options);

            workflowProcess.Completion.ContinueWith(t => Console.WriteLine($"!!!Task {options.NameFormat} is complete!!!"));
        }
    }
}
