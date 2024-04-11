using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <see cref="IndianaEnumerations.PhaseBeginGreen"/> through <see cref="IndianaEnumerations.Split16Change"/>
    /// </summary>
    public class FilteredSplitsData : FilterEventCodeBase
    {
        /// <inheritdoc/>
        public FilteredSplitsData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            for (int i = (int)IndianaEnumerations.Split1Change; i <= (int)IndianaEnumerations.Split16Change; i++)
            {
                filteredList.Add(i);
            }
        }
    }
}
