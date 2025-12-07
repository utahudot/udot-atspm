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

namespace Utah.Udot.Atspm.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="131"/></item>
    /// </list>
    /// </summary>
    public class FilteredSplitFail : FilterEventCodeLocationBase
    {
        /// <inheritdoc/>
        public FilteredSplitFail(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add(1);
            filteredList.Add(4);
            filteredList.Add(5);
            filteredList.Add(6);
            filteredList.Add(8);
            filteredList.Add(9);
            filteredList.Add(61);
            filteredList.Add(63);
            filteredList.Add(64);
            filteredList.Add(66);
            filteredList.Add(81);
            filteredList.Add(82);
        }
    }
}
