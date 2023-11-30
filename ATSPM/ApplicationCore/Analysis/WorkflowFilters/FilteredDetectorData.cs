using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Workflows;
using System.Collections.Generic;
using System;
using System.Threading.Tasks.Dataflow;
using System.Linq;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="DataLoggerEnum.DetectorOff"/></item>
    /// <item><see cref="DataLoggerEnum.DetectorOn"/></item>
    /// </list>
    /// </summary>
    public class FilteredDetectorData : ProcessStepBase<Tuple<Approach, IEnumerable<ControllerEventLog>>, Tuple<Approach, IEnumerable<ControllerEventLog>>>
    {
        /// <summary>
        /// List of filtered event codes
        /// </summary>
        protected List<int> filteredList = new();

        /// <inheritdoc/>
        public FilteredDetectorData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.DetectorOff);
            filteredList.Add((int)DataLoggerEnum.DetectorOn);

            workflowProcess = new BroadcastBlock<Tuple<Approach, IEnumerable<ControllerEventLog>>>(f =>
            {
                var dc = f.Item1.Detectors.Select(s => s.DetectorChannel).ToList();

                var result = Tuple.Create(f.Item1, f.Item2.Where(w => w.SignalIdentifier == f.Item1.Signal.SignalIdentifier && filteredList.Contains(w.EventCode) && dc.Contains(w.EventParam)));

                return result;

            }, options);
            workflowProcess.Completion.ContinueWith(t => Console.WriteLine($"!!!Task {options.NameFormat} is complete!!!"));
        }
    }
}
