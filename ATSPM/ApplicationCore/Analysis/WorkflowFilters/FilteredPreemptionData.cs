#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.WorkflowFilters/FilteredPreemptionData.cs
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
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="102"/></item>
    /// <item><see cref="103"/></item>
    /// <item><see cref="104"/></item>
    /// <item><see cref="105"/></item>
    /// <item><see cref="106"/></item>
    /// <item><see cref="107"/></item>
    /// <item><see cref="110"/></item>
    /// <item><see cref="111"/></item>
    /// </list>
    /// </summary>
    public class FilteredPreemptionData : FilterEventCodeLocationBase
    {
        /// <inheritdoc/>
        public FilteredPreemptionData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)102);
            filteredList.Add((int)103);
            filteredList.Add((int)104);
            filteredList.Add((int)105);
            filteredList.Add((int)106);
            filteredList.Add((int)107);
            filteredList.Add((int)110);
            filteredList.Add((int)111);
        }
    }
}
