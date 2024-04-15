using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="43"/></item>
    /// <item><see cref="44"/></item>
    /// </list>
    /// </summary>
    public class FilteredCallStatus : FilterEventCodeBase
    {
        /// <inheritdoc/>
        public FilteredCallStatus(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)43);
            filteredList.Add((int)44);
        }
    }
}
