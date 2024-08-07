﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.WorkflowFilters/FilterTspPriorityData.cs
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
using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="112"/></item>
    /// <item><see cref="113"/></item>
    /// <item><see cref="114"/></item>
    /// <item><see cref="IndianaEnumerations.TSPCheckOut"/></item>
    /// </list>
    /// </summary>
    public class FilterTspPriorityData : FilterEventCodeLocationBase
    {
        /// <inheritdoc/>
        public FilterTspPriorityData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add(112);
            filteredList.Add(113);
            filteredList.Add(114);
            filteredList.Add((int)IndianaEnumerations.TSPCheckOut);
        }
    }
}
