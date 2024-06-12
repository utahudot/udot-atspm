#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.WorkflowFilters/FilterEventCodeBase.cs
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
using ATSPM.Data.Models;
using ATSPM.Domain.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Base class for filter controller event log data used in process workflows
    /// </summary>
    public abstract class FilterEventCodeBase : ProcessStepBase<IEnumerable<ControllerEventLog>, IEnumerable<ControllerEventLog>>
    {
        /// <summary>
        /// List of filtered event codes
        /// </summary>
        protected List<int> filteredList = new();

        /// <inheritdoc/>
        public FilterEventCodeBase(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            workflowProcess = new BroadcastBlock<IEnumerable<ControllerEventLog>>(f => f.Where(l => filteredList.Contains(l.EventCode)), options);
            workflowProcess.Completion.ContinueWith(t => Console.WriteLine($"!!!Task {options.NameFormat} is complete!!!"));
        }
    }
}
