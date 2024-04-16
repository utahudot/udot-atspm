using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="45"/></item>
    /// <item><see cref="90"/></item>
    /// </list>
    /// </summary>
    public class FilteredPedCalls : FilterEventCodeBase
    {
        /// <inheritdoc/>
        public FilteredPedCalls(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)45);
            filteredList.Add((int)90);
        }
    }
}
