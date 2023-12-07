using ATSPM.Application.Specifications;
using ATSPM.Data.Models;
using ATSPM.Domain.Extensions;
using ATSPM.Domain.Workflows;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class GroupApproachesByPhase : TransformManyProcessStepBase<Tuple<Approach, IEnumerable<ControllerEventLog>>, Tuple<Approach, int, IEnumerable<ControllerEventLog>>>
    {
        public GroupApproachesByPhase(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<Tuple<Approach, int, IEnumerable<ControllerEventLog>>>> Process(Tuple<Approach, IEnumerable<ControllerEventLog>> input, CancellationToken cancelToken = default)
        {
            var result = input.Item2
                .GroupBy(g => g.EventParam, (phase, i) => Tuple.Create(input.Item1, phase, i))
                .Where(w => w.Item1.ProtectedPhaseNumber == w.Item2);

            return Task.FromResult(result);
        }
    }
}
