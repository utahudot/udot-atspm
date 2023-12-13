using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="DataLoggerEnum.CoordPatternChange"/></item>
    /// </list>
    /// </summary>
    public class FilteredPlanData : FilterEventCodeLocationBase
    {
        /// <inheritdoc/>
        public FilteredPlanData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.CoordPatternChange);
        }
    }
}
