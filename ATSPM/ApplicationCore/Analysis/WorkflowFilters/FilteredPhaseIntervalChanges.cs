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
