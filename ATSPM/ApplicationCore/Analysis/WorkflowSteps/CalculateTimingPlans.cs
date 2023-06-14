using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.Plans;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using ATSPM.Domain.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class CalculateTimingPlans<T> : TransformManyProcessStepBase<IEnumerable<ControllerEventLog>, IReadOnlyList<T>> where T : IPlan, new()
    {
        public CalculateTimingPlans(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<IReadOnlyList<T>>> Process(IEnumerable<ControllerEventLog> input, CancellationToken cancelToken = default)
        {
            var result = input
                .Where(w => w.EventCode == (int)DataLoggerEnum.CoordPatternChange)
                .OrderBy(o => o.Timestamp)
                .GroupBy(g => g.SignalId, (k, i) => i
                .GroupBy(p => p.EventParam, (n, c) => c
                .Where((w, i) => i < c.Count() - 1)
                .Select((s, i) => new T() { SignalIdentifier = k, PlanNumber = n, Start = c.ElementAt(i).Timestamp, End = c.ElementAt(i + 1).Timestamp }))
                .SelectMany(s => s).ToList());

            if (result.Count() == 0)
                result = new List<List<T>>() { new List<T>() };


            return Task.FromResult<IEnumerable<IReadOnlyList<T>>>(result);
        }
    }
}
