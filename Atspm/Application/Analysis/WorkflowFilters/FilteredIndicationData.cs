#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - %Namespace%/FilteredIndicationData.cs
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
    /// <item><see cref="1"/></item>
    /// <item><see cref="10"/></item>
    /// </list>
    /// </summary>
    public class FilteredIndicationData : FilterEventCodeBase
    {
        /// <inheritdoc/>
        public FilteredIndicationData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add(1);
            filteredList.Add(10);
        }
    }
}
