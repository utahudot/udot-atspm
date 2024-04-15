using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="102"/></item>
    /// <item><see cref="103"/></item>
    /// <item><see cref="104"/></item>
    /// <item><see cref="105"/></item>
    /// <item><see cref="106"/></item>
    /// <item><see cref="107"/></item>
    /// <item><see cref="110"/></item>
    /// <item><see cref="111"/></item>
    /// </list>
    /// </summary>
    public class FilteredPreemptionData : FilterEventCodeLocationBase
    {
        /// <inheritdoc/>
        public FilteredPreemptionData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)102);
            filteredList.Add((int)103);
            filteredList.Add((int)104);
            filteredList.Add((int)105);
            filteredList.Add((int)106);
            filteredList.Add((int)107);
            filteredList.Add((int)110);
            filteredList.Add((int)111);
        }
    }
}
