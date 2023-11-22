using ATSPM.Application.Specifications;
using ATSPM.Data.Models;
using ATSPM.Domain.Extensions;
using ATSPM.Domain.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class FilterApproachesFromSignal : TransformManyProcessStepBase<Tuple<Signal, IEnumerable<ControllerEventLog>>, Tuple<Approach, IEnumerable<ControllerEventLog>>>
    {
        public FilterApproachesFromSignal(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<Tuple<Approach, IEnumerable<ControllerEventLog>>>> Process(Tuple<Signal, IEnumerable<ControllerEventLog>> input, CancellationToken cancelToken = default)
        {
            var result = input.Item1.Approaches.Select(s => Tuple.Create(s, input.Item2.FromSpecification(new ControllerLogSignalFilterSpecification(input.Item1))));

            return Task.FromResult(result);
        }
    }
}
