using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="IndianaEnumerations.PhaseCallRegistered"/></item>
    /// <item><see cref="IndianaEnumerations.PhaseCallDropped"/></item>
    /// </list>
    /// </summary>
    public class FilteredCallStatus : FilterEventCodeBase
    {
        /// <inheritdoc/>
        public FilteredCallStatus(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)IndianaEnumerations.PhaseCallRegistered);
            filteredList.Add((int)IndianaEnumerations.PhaseCallDropped);
        }
    }
}
