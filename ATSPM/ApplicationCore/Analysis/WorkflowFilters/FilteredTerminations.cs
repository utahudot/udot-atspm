using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="DataLoggerEnum.PhaseGapOut"/></item>
    /// <item><see cref="DataLoggerEnum.PhaseMaxOut"/></item>
    /// <item><see cref="DataLoggerEnum.PhaseForceOff"/></item>
    /// </list>
    /// </summary>
    public class FilteredTerminations : FilterEventCodeBase
    {
        /// <inheritdoc/>
        public FilteredTerminations(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.PhaseGapOut);
            filteredList.Add((int)DataLoggerEnum.PhaseMaxOut);
            filteredList.Add((int)DataLoggerEnum.PhaseForceOff);
        }
    }
}
