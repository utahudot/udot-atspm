using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <see cref="1"/> through <see cref="149"/>
    /// </summary>
    public class FilteredSplitsData : FilterEventCodeBase
    {
        /// <inheritdoc/>
        public FilteredSplitsData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            for (int i = (int)134; i <= (int)149; i++)
            {
                filteredList.Add(i);
            }
        }
    }
}
