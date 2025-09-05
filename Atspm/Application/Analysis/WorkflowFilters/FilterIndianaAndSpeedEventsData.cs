#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowFilters/FilteredPlanData.cs
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
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="131"/></item>
    /// </list>
    /// </summary>
    public class FilterIndianaAndSpeedEventsData : ProcessStepBase<Tuple<Location, Tuple<IEnumerable<IndianaEvent>, IEnumerable<SpeedEvent>>>, Tuple<Location, Tuple<IEnumerable<IndianaEvent>, IEnumerable<SpeedEvent>>>>
    {
        /// <summary>
        /// List of filtered event codes
        /// </summary>
        protected List<int> filteredList = new();
        /// <inheritdoc/>
        public FilterIndianaAndSpeedEventsData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            workflowProcess = new BroadcastBlock<Tuple<Location, Tuple<IEnumerable<IndianaEvent>, IEnumerable<SpeedEvent>>>>(f =>
            {
                Tuple<IEnumerable<IndianaEvent>, IEnumerable<SpeedEvent>> tuple = Tuple.Create(f.Item2.Item1
                     .FromSpecification(new IndianaLogLocationFilterSpecification(f.Item1))
                     .Where(w => filteredList.Contains(w.EventCode)), f.Item2.Item2.FromSpecification(new SpeedLogLocationFilterSpecification(f.Item1)));

                return Tuple.Create(f.Item1, tuple);
            }, options);
            workflowProcess.Completion.ContinueWith(t => Console.WriteLine($"!!!Task {options.NameFormat} is complete!!!"));
        }
    }
}
