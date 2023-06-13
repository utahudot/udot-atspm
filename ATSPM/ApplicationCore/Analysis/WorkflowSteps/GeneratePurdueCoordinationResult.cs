using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.Plans;
using ATSPM.Application.Analysis.PurdueCoordination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class GeneratePurdueCoordinationResult : TransformManyProcessStepBase<IReadOnlyList<CycleArrivals>, PurdueCoordinationResult>
    {
        public GeneratePurdueCoordinationResult(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<PurdueCoordinationResult>> Process(IReadOnlyList<CycleArrivals> input, CancellationToken cancelToken = default)
        {
            var result = input.GroupBy(g => g.SignalIdentifier, (signal, x) =>
            x.GroupBy(g => g.PhaseNumber, (phase, y) => new PurdueCoordinationResult()
            {
                SignalIdentifier = signal,
                PhaseNumber = phase,
                Start = y.Min(m => m.Start),
                End = y.Max(m => m.End),
                Plans = new List<PurdueCoordinationPlan>() {
                    new PurdueCoordinationPlan()
                    {
                        SignalIdentifier = signal,
                        Start = y.Min(m => m.Start),
                        End = y.Max(m => m.End),
                        ArrivalCycles = y.ToList()
                    }
                }
            })).SelectMany(m => m);

            return Task.FromResult(result);
        }
    }
}
