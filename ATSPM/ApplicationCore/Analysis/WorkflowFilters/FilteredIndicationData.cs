using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="1"/></item>
    /// <item><see cref="10"/></item>
    /// </list>
    /// </summary>
    public class FilteredIndicationData : FilterEventCodeBase
    {
        /// <inheritdoc/>
        public FilteredIndicationData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)1);
            filteredList.Add((int)10);
        }
    }
}
