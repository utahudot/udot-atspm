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
    public class GroupLocationByParameter : TransformManyProcessStepBase<Tuple<Location, IEnumerable<ControllerEventLog>>, Tuple<Location, int, IEnumerable<ControllerEventLog>>>
    {
        public GroupLocationByParameter(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<Tuple<Location, int, IEnumerable<ControllerEventLog>>>> Process(Tuple<Location, IEnumerable<ControllerEventLog>> input, CancellationToken cancelToken = default)
        {
            var result = input.Item2
                .FromSpecification(new ControllerLogLocationFilterSpecification(input.Item1))
                .GroupBy(g => g.EventParam)
                .Select(s => Tuple.Create(input.Item1, s.Key, s.AsEnumerable()));

            return Task.FromResult(result);
        }
    }
}
