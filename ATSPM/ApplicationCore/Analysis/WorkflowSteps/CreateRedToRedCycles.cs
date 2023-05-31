using ATSPM.Application.Analysis.Common;
using ATSPM.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    //public class GroupEventsBySignalId : TransformManyProcessStepBase<IEnumerable<ControllerEventLog>, IEnumerable<ControllerEventLog>>
    //{
    //    public GroupEventsBySignalId(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

    //    protected override Task<IEnumerable<IEnumerable<ControllerEventLog>>> Process(IEnumerable<ControllerEventLog> input, CancellationToken cancelToken = default)
    //    {
    //        var result = input.GroupBy(g => g.SignalId).Select(s => s.AsEnumerable());

    //        return Task.FromResult(result);
    //    }
    //}

    //public class GroupEventsByEventParam : TransformManyProcessStepBase<IEnumerable<ControllerEventLog>, IEnumerable<ControllerEventLog>>
    //{
    //    public GroupEventsByEventParam(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

    //    protected override Task<IEnumerable<IEnumerable<ControllerEventLog>>> Process(IEnumerable<ControllerEventLog> input, CancellationToken cancelToken = default)
    //    {
    //        var result = input.GroupBy(g => g.EventParam).Select(s => s.AsEnumerable());

    //        return Task.FromResult(result);
    //    }
    //}

    public class CreateRedToRedCycles : TransformProcessStepBase<IEnumerable<ControllerEventLog>, IEnumerable<RedToRedCycle>>
    {
        public CreateRedToRedCycles(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<RedToRedCycle>> Process(IEnumerable<ControllerEventLog> input, CancellationToken cancelToken = default)
        {
            var result = new List<RedToRedCycle>();

            var signalFilter = input.Where(l => l.EventCode == 1 || l.EventCode == 8 || l.EventCode == 9)
                .OrderBy(o => o.Timestamp)
                .GroupBy(g => g.SignalId);

            foreach (var signal in signalFilter)
            {
                foreach (var phase in signal.GroupBy(g => g.EventParam))
                {
                    var items = phase.Select(s => s).ToList();
                    if (items.Count > 3)
                    {
                        var group = items
                        .Where((w, i) => i <= items.Count - 3 && w.EventCode == 9 && items[i + 1].EventCode == 1 && items[i + 2].EventCode == 8 && items[i + 3].EventCode == 9)
                        .Select((s, i) => new { s, i = items.IndexOf(s) })
                        .Select(s => items.Skip(s.i).Take(4))
                        .Select(s => new RedToRedCycle()
                        {
                            Start = s.ElementAt(0).Timestamp,
                            End = s.ElementAt(3).Timestamp,
                            GreenEvent = s.ElementAt(1).Timestamp,
                            YellowEvent = s.ElementAt(2).Timestamp,
                            Phase = phase.Key,
                            SignalId = signal.Key
                        });

                        result.AddRange(group);
                    }
                }
            }

            return Task.FromResult<IEnumerable<RedToRedCycle>>(result);
        }
    }
}
