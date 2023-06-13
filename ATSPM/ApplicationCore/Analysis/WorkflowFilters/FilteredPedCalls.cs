using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="DataLoggerEnum.PedestrianCallRegistered"/></item>
    /// <item><see cref="DataLoggerEnum.PedDetectorOn"/></item>
    /// </list>
    /// </summary>
    public class FilteredPedCalls : FilterStepBase
    {
        public FilteredPedCalls(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.PedestrianCallRegistered);
            filteredList.Add((int)DataLoggerEnum.PedDetectorOn);
        }
    }
}
