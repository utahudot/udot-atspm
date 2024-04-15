using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="IndianaEnumerations.PhaseGreenTermination"/></item>
    /// </list>
    /// </summary>
    public class FilteredTerminationStatus : FilterEventCodeBase
    {
        /// <inheritdoc/>
        public FilteredTerminationStatus(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)IndianaEnumerations.PhaseGreenTermination);
        }
    }
}
