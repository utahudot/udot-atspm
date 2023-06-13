using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="DataLoggerEnum.PhaseBeginGreen"/></item>
    /// <item><see cref="DataLoggerEnum.PhaseBeginRedClearance"/></item>
    /// </list>
    /// </summary>
    public class FilteredIndicationData : FilterStepBase
    {
        public FilteredIndicationData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.PhaseBeginGreen);
            filteredList.Add((int)DataLoggerEnum.PhaseBeginRedClearance);
        }
    }
}
