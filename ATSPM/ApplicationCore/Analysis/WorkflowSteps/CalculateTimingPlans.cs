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
    public class CalculateTimingPlans<T> : TransformProcessStepBase<Tuple<Signal, int, IEnumerable<ControllerEventLog>>, Tuple<Signal, int, IEnumerable<T>>> where T : IPlan, new()
    {
        public CalculateTimingPlans(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<Tuple<Signal, int, IEnumerable<T>>> Process(Tuple<Signal, int, IEnumerable<ControllerEventLog>> input, CancellationToken cancelToken = default)
        {
            var result = Tuple.Create(input.Item1, input.Item2, input.Item3
                .GroupBy(g => g.EventCode, (k, l) => 
                l.Select((s, i) => new T()
                {
                    SignalIdentifier = input.Item1.SignalIdentifier,
                    PlanNumber = input.Item2,
                    Start = l.ElementAt(i).Timestamp,
                    End = i < l.Count() - 1 ? l.ElementAt(i + 1).Timestamp : default
                }))
                .SelectMany(m => m));

            return Task.FromResult(result);
        }
    }
}
