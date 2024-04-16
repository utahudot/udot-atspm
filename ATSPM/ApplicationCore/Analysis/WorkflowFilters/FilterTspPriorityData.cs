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
    /// <item><see cref="IndianaEnumerations.TSPCheckOut"/></item>
    /// </list>
    /// </summary>
    public class FilterTspPriorityData : FilterEventCodeLocationBase
    {
        /// <inheritdoc/>
        public FilterTspPriorityData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)112);
            filteredList.Add((int)113);
            filteredList.Add((int)114);
            filteredList.Add((int)IndianaEnumerations.TSPCheckOut);
        }
    }
}
