using ATSPM.Application.Analysis.PreemptionDetails;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class GeneratePreemptDetailResults : TransformManyProcessStepBase<IEnumerable<PreempDetailValueBase>, PreemptDetailResult>
    {
        public GeneratePreemptDetailResults(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<PreemptDetailResult>> Process(IEnumerable<PreempDetailValueBase> input, CancellationToken cancelToken = default)
        {
            var result = new List<PreemptDetailResult>();

            foreach (var signal in input.GroupBy(g => g.SignalId))
            {
                foreach (var item in signal.GroupBy(g => g.PreemptNumber))
                {
                    result.Add(new PreemptDetailResult()
                    {
                        SignalId = signal.Key,
                        PreemptNumber = item.Key,
                        Start = item.Min(m => m.Start),
                        End = item.Max(m => m.End),
                        DwellTimes = item.Where(w => w.GetType().Name == nameof(DwellTimeValue)).Cast<DwellTimeValue>(),
                        TrackClearTimes = item.Where(w => w.GetType().Name == nameof(TrackClearTimeValue)).Cast<TrackClearTimeValue>(),
                        ServiceTimes = item.Where(w => w.GetType().Name == nameof(TimeToServiceValue)).Cast<TimeToServiceValue>(),
                        Delay = item.Where(w => w.GetType().Name == nameof(DelayTimeValue)).Cast<DelayTimeValue>(),
                        GateDownTimes = item.Where(w => w.GetType().Name == nameof(TimeToGateDownValue)).Cast<TimeToGateDownValue>(),
                        CallMaxOutTimes = item.Where(w => w.GetType().Name == nameof(TimeToCallMaxOutValue)).Cast<TimeToCallMaxOutValue>()
                    });
                }
            }

            return Task.FromResult<IEnumerable<PreemptDetailResult>>(result);
        }
    }
}
