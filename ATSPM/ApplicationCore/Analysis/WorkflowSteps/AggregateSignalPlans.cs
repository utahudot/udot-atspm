using ATSPM.Application.Analysis.Plans;
using ATSPM.Data.Models;
using ATSPM.Domain.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class AggregateLocationPlans : TransformProcessStepBase<Tuple<Location, int, IEnumerable<Plan>>, IEnumerable<LocationPlanAggregation>>
    {
        public AggregateLocationPlans(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<LocationPlanAggregation>> Process(Tuple<Location, int, IEnumerable<Plan>> input, CancellationToken cancelToken = default)
        {
            var result = input.Item3
                .Select(s => new LocationPlanAggregation()
                {
                    LocationIdentifier = s.LocationIdentifier,
                    PlanNumber = s.PlanNumber,
                    Start = s.Start,
                    End = s.End,
                });

            return Task.FromResult(result);
        }
    }
}
