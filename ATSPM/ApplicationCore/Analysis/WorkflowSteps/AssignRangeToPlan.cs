using ATSPM.Application.Analysis.Plans;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
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
            foreach (var p in input.Item1)
            {
                foreach (var r in input.Item2)
                {
                    p.TryAssignToPlan(r);
                }
            }

            return Task.FromResult(input.Item1);
        }
    }
}
