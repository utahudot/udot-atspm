#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.WorkflowFilters/FilteredPhaseIntervalChanges.cs
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
using ATSPM.Domain.Workflows;
using System.Collections.Generic;
using System;
using System.Threading.Tasks.Dataflow;
using System.Linq;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="1"/></item>
    /// <item><see cref="8"/></item>
    /// <item><see cref="9"/></item>
    /// <item><see cref="11"/></item>
    /// </list>
    /// </summary>
    public class FilteredPhaseIntervalChanges : FilterEventCodeLocationBase
    {
        /// <inheritdoc/>
        public FilteredPhaseIntervalChanges(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)1);
            filteredList.Add((int)8);
            filteredList.Add((int)9);
            filteredList.Add((int)11);
        }
    }
}
