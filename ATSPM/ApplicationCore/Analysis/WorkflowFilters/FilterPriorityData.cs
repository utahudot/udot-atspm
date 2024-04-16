using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="112"/></item>
    /// <item><see cref="113"/></item>
    /// <item><see cref="114"/></item>
    /// </list>
    /// </summary>
    public class FilterPriorityData : FilterEventCodeLocationBase
    {
        /// <inheritdoc/>
        public FilterPriorityData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)112);
            filteredList.Add((int)113);
            filteredList.Add((int)114);
        }
    }
}
