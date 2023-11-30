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
    public class AggregateSignalPlans : TransformProcessStepBase<Tuple<Signal, int, IEnumerable<Plan>>, Tuple<Signal, int, IEnumerable<SignalPlanAggregation>>>
    {
        public AggregateSignalPlans(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<Tuple<Signal, int, IEnumerable<SignalPlanAggregation>>> Process(Tuple<Signal, int, IEnumerable<Plan>> input, CancellationToken cancelToken = default)
        {
            var result = Tuple.Create(input.Item1, input.Item2, input.Item3
                .Select(s => new SignalPlanAggregation()
                {
                    SignalIdentifier = s.SignalIdentifier,
                    PlanNumber = s.PlanNumber,
                    Start = s.Start,
                    End = s.End,
                }));

            return Task.FromResult(result);
        }
    }
}
