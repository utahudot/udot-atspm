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
    /// <item><see cref="IndianaEnumerations.PhaseBeginGreen"/></item>
    /// <item><see cref="IndianaEnumerations.PhaseBeginYellowChange"/></item>
    /// <item><see cref="IndianaEnumerations.PhaseEndYellowChange"/></item>
    /// <item><see cref="IndianaEnumerations.PhaseEndRedClearance"/></item>
    /// </list>
    /// </summary>
    public class FilteredPhaseIntervalChanges : FilterEventCodeLocationBase
    {
        /// <inheritdoc/>
        public FilteredPhaseIntervalChanges(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)IndianaEnumerations.PhaseBeginGreen);
            filteredList.Add((int)IndianaEnumerations.PhaseBeginYellowChange);
            filteredList.Add((int)IndianaEnumerations.PhaseEndYellowChange);
            filteredList.Add((int)IndianaEnumerations.PhaseEndRedClearance);
        }
    }
}
