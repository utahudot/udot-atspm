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
    /// <item><see cref="DataLoggerEnum.PhaseBeginGreen"/></item>
    /// <item><see cref="DataLoggerEnum.PhaseBeginYellowChange"/></item>
    /// <item><see cref="DataLoggerEnum.PhaseEndYellowChange"/></item>
    /// <item><see cref="DataLoggerEnum.PhaseEndRedClearance"/></item>
    /// </list>
    /// </summary>
    public class FilteredPhaseIntervalChanges : FilterEventCodeSignalBase
    {
        /// <inheritdoc/>
        public FilteredPhaseIntervalChanges(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.PhaseBeginGreen);
            filteredList.Add((int)DataLoggerEnum.PhaseBeginYellowChange);
            filteredList.Add((int)DataLoggerEnum.PhaseEndYellowChange);
            filteredList.Add((int)DataLoggerEnum.PhaseEndRedClearance);
        }
    }
}
