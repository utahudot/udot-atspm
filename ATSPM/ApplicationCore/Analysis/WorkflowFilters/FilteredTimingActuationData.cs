using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="1"/></item>
    /// <item><see cref="3"/></item>
    /// <item><see cref="5"/></item>
    /// <item><see cref="9"/></item>
    /// <item><see cref="11"/></item>
    /// <item><see cref="21"/></item>
    /// <item><see cref="22"/></item>
    /// <item><see cref="23"/></item>
    /// <item><see cref="61"/></item>
    /// <item><see cref="62"/></item>
    /// <item><see cref="63"/></item>
    /// <item><see cref="64"/></item>
    /// <item><see cref="65"/></item>
    /// <item><see cref="67"/></item>
    /// <item><see cref="68"/></item>
    /// <item><see cref="69"/></item>
    /// <item><see cref="81"/></item>
    /// <item><see cref="IndianaEnumerations.VehicleDetectorOn"/></item>
    /// <item><see cref="89"/></item>
    /// <item><see cref="90"/></item>
    /// </list>
    /// </summary>
    public class FilteredTimingActuationData : FilterEventCodeBase
    {
        /// <inheritdoc/>
        public FilteredTimingActuationData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)1);
            filteredList.Add((int)3);
            filteredList.Add((int)5);
            filteredList.Add((int)9);
            filteredList.Add((int)11);
            filteredList.Add((int)21);
            filteredList.Add((int)22);
            filteredList.Add((int)23);
            filteredList.Add((int)61);
            filteredList.Add((int)62);
            filteredList.Add((int)63);
            filteredList.Add((int)64);
            filteredList.Add((int)65);
            filteredList.Add((int)67);
            filteredList.Add((int)68);
            filteredList.Add((int)69);
            filteredList.Add((int)81);
            filteredList.Add((int)IndianaEnumerations.VehicleDetectorOn);
            filteredList.Add((int)89);
            filteredList.Add((int)90);
        }
    }
}
