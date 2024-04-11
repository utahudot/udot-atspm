using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="IndianaEnumerations.PhaseBeginGreen"/></item>
    /// <item><see cref="IndianaEnumerations.PhaseBeginRedClearance"/></item>
    /// </list>
    /// </summary>
    public class FilteredIndicationData : FilterEventCodeBase
    {
        /// <inheritdoc/>
        public FilteredIndicationData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)IndianaEnumerations.PhaseBeginGreen);
            filteredList.Add((int)IndianaEnumerations.PhaseBeginRedClearance);
        }
    }
}
