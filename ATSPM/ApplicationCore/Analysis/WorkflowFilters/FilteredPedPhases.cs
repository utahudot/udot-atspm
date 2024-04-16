using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="21"/></item>
    /// <item><see cref="23"/></item>
    /// </list>
    /// </summary>
    public class FilteredPedPhases : FilterEventCodeBase
    {
        /// <inheritdoc/>
        public FilteredPedPhases(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)21);
            filteredList.Add((int)23);
        }
    }
}
