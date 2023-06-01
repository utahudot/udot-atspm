using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.PurdueCoordination;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
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
    public class Plan : StartEndRange
    {
        public string SignalId { get; set; }
        public int PlanNumber { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    public class CalculateTimingPlans : TransformManyProcessStepBase<IEnumerable<ControllerEventLog>, IReadOnlyList<Plan>>
    {
        public CalculateTimingPlans(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<IReadOnlyList<Plan>>> Process(IEnumerable<ControllerEventLog> input, CancellationToken cancelToken = default)
        {
            var result = input
                .Where(w => w.EventCode == (int)DataLoggerEnum.CoordPatternChange)
                .OrderBy(o => o.Timestamp)
                .GroupBy(g => g.SignalId, (k, i) => i
                .GroupBy(p => p.EventParam, (n, c) => c
                .Where((w, i) => i < c.Count() - 1)
                .Select((s, i) => new Plan() { SignalId = k, PlanNumber = n, Start = c.ElementAt(i).Timestamp, End = c.ElementAt(i + 1).Timestamp }))
                .SelectMany(s => s).ToList());
                //.SelectMany(s => s)
                //.ToList();

            return Task.FromResult<IEnumerable<IReadOnlyList<Plan>>>(result);
        }
    }
}
