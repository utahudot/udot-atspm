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
    /// <item><see cref="DataLoggerEnum.PhaseBeginGreen"/></item>
    /// <item><see cref="DataLoggerEnum.PhaseBeginYellowChange"/></item>
    /// <item><see cref="DataLoggerEnum.PhaseEndYellowChange"/></item>
    /// <item><see cref="DataLoggerEnum.PhaseEndRedClearance"/></item>
    /// </list>
    /// </summary>
    public class FilteredPhaseIntervalChanges : ProcessStepBase<Tuple<Approach, IEnumerable<ControllerEventLog>>, Tuple<Approach, IEnumerable<ControllerEventLog>>>
    {
        /// <summary>
        /// List of filtered event codes
        /// </summary>
        protected List<int> filteredList = new();

        /// <inheritdoc/>
        public FilteredPhaseIntervalChanges(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.PhaseBeginGreen);
            filteredList.Add((int)DataLoggerEnum.PhaseBeginYellowChange);
            filteredList.Add((int)DataLoggerEnum.PhaseEndYellowChange);
            filteredList.Add((int)DataLoggerEnum.PhaseEndRedClearance);

            workflowProcess = new BroadcastBlock<Tuple<Approach, IEnumerable<ControllerEventLog>>>(f =>
            {
                //TODO: get the correct phase from approach here
                var result = Tuple.Create(f.Item1, f.Item2.Where(w => w.SignalIdentifier == f.Item1.Signal.SignalIdentifier && filteredList.Contains(w.EventCode) && w.EventParam == f.Item1.ProtectedPhaseNumber));

                return result;

            }, options);
            workflowProcess.Completion.ContinueWith(t => Console.WriteLine($"!!!Task {options.NameFormat} is complete!!!"));
        }
    }

    //public class FilteredPhaseIntervalChanges : FilterStepBase
    //{
    //    /// <inheritdoc/>
    //    public FilteredPhaseIntervalChanges(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
    //    {
    //        filteredList.Add((int)DataLoggerEnum.PhaseBeginGreen);
    //        filteredList.Add((int)DataLoggerEnum.PhaseBeginYellowChange);
    //        filteredList.Add((int)DataLoggerEnum.PhaseEndYellowChange);
    //        filteredList.Add((int)DataLoggerEnum.PhaseEndRedClearance);
    //    }
    //}
}
