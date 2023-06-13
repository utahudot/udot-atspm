using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <see cref="DataLoggerEnum.PhaseBeginGreen"/> through <see cref="DataLoggerEnum.Split16Change"/>
    /// </summary>
    public class FilteredSplitsData : FilterStepBase
    {
        public FilteredSplitsData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            for (int i = (int)DataLoggerEnum.Split1Change; i <= (int)DataLoggerEnum.Split16Change; i++)
            {
                filteredList.Add(i);
            }
        }
    }
}
