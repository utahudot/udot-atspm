using ATSPM.Application.Analysis.PreemptionDetails;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public abstract class PreemptiveProcessBase<T> : TransformManyProcessStepBase<IEnumerable<ControllerEventLog>, IEnumerable<T>> where T : PreempDetailValueBase, new()
    {
        protected DataLoggerEnum first;
        protected DataLoggerEnum second;

        public PreemptiveProcessBase(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<IEnumerable<T>>> Process(IEnumerable<ControllerEventLog> input, CancellationToken cancelToken = default)
        {
            var result = input.GroupBy(g => g.SignalId)
                .SelectMany(s => s.GroupBy(g => g.EventParam)
                .Select(s => s.TimeSpanFromConsecutiveCodes(first, second)
                .Select(s => new T()
                {
                    SignalId = s.Item1[0].SignalId == s.Item1[1].SignalId ? s.Item1[0].SignalId : string.Empty,
                    PreemptNumber = Convert.ToInt32(s.Item1.Average(a => a.EventParam)),
                    Start = s.Item1[0].Timestamp,
                    End = s.Item1[1].Timestamp,
                    Seconds = s.Item2
                })));

            return Task.FromResult(result);
        }
    }
}
