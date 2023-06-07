using ATSPM.Application.Analysis.Common;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class CreateRedToRedCycles : TransformProcessStepBase<IEnumerable<ControllerEventLog>, IReadOnlyList<RedToRedCycle>>
    {
        public CreateRedToRedCycles(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IReadOnlyList<RedToRedCycle>> Process(IEnumerable<ControllerEventLog> input, CancellationToken cancelToken = default)
        {
            var result = input.Where(l => l.EventCode == 1 || l.EventCode == 8 || l.EventCode == 9)
                .OrderBy(o => o.Timestamp)
                .GroupBy(g => g.SignalId, (s, x) => x
                .GroupBy(g => g.EventParam, (p, y) => y    
                .Where((w, i) => y.Count() > 3 && i <= y.Count() - 3)
                .Where((w, i) => w.EventCode == 9 && y.ElementAt(i + 1).EventCode == 1 && y.ElementAt(i + 2).EventCode == 8 && y.ElementAt(i + 3).EventCode == 9)
                .Select((s, i) => y.Skip(y.ToList().IndexOf(s)).Take(4))
                .Select(m => new RedToRedCycle()
                {
                    Start = m.ElementAt(0).Timestamp,
                    End = m.ElementAt(3).Timestamp,
                    GreenEvent = m.ElementAt(1).Timestamp,
                    YellowEvent = m.ElementAt(2).Timestamp,
                    PhaseNumber = p,
                    SignalIdentifier = s
                }))
                .SelectMany(m => m))
                .SelectMany(m => m)
                .ToList();

            return Task.FromResult<IReadOnlyList<RedToRedCycle>>(result);
        }
    }
}
