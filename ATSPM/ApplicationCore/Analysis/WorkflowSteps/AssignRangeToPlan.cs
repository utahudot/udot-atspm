using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.Plans;
using ATSPM.Data.Interfaces;
using ATSPM.Domain.Common;
using ATSPM.Domain.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class AssignRangeToPlan<T> : TransformProcessStepBase<Tuple<IReadOnlyList<T>, IReadOnlyList<IStartEndRange>>, IReadOnlyList<T>> where T : IPlan, new()
    {
        public AssignRangeToPlan(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IReadOnlyList<T>> Process(Tuple<IReadOnlyList<T>, IReadOnlyList<IStartEndRange>> input, CancellationToken cancelToken = default)
        {
            List<T> plans;

            if (input.Item1.Count == 0)
            {
                plans = input.Item2.Cast<ISignalLayer>().GroupBy(g => g.SignalIdentifier, (s, i) => new T()
                {
                    SignalIdentifier = s,
                    Start = i.Cast<IStartEndRange>().Min(m => m.Start),
                    End = i.Cast<IStartEndRange>().Max(m => m.End)
                }).ToList();
            }
            else
            {
                plans = input.Item1.ToList();
            }

            foreach (var p in plans)
            {
                p.AssignToPlan(input.Item2);
            }

            return Task.FromResult<IReadOnlyList<T>>(plans);
        }
    }
}
