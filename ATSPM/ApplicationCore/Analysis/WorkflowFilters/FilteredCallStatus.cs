using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="DataLoggerEnum.PhaseCallRegistered"/></item>
    /// <item><see cref="DataLoggerEnum.PhaseCallDropped"/></item>
    /// </list>
    /// </summary>
    public class FilteredCallStatus : FilterEventCodeBase
    {
        /// <inheritdoc/>
        public FilteredCallStatus(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.PhaseCallRegistered);
            filteredList.Add((int)DataLoggerEnum.PhaseCallDropped);
        }
    }
}
