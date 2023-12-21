using ATSPM.Application.Analysis.PreemptionDetails;
using ATSPM.Domain.Workflows;
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

            foreach (var Location in input.GroupBy(g => g.LocationIdentifier))
            {
                foreach (var item in Location.GroupBy(g => g.PreemptNumber))
                {
                    result.Add(new PreemptDetailResult()
                    {
                        LocationIdentifier = Location.Key,
                        PreemptNumber = item.Key,
                        Start = item.Min(m => m.Start),
                        End = item.Max(m => m.End),
                        DwellTimes = item.Where(w => w.GetType().Name == nameof(DwellTimeValue)).Cast<DwellTimeValue>().ToList(),
                        TrackClearTimes = item.Where(w => w.GetType().Name == nameof(TrackClearTimeValue)).Cast<TrackClearTimeValue>().ToList(),
                        ServiceTimes = item.Where(w => w.GetType().Name == nameof(TimeToServiceValue)).Cast<TimeToServiceValue>().ToList(),
                        Delay = item.Where(w => w.GetType().Name == nameof(DelayTimeValue)).Cast<DelayTimeValue>().ToList(),
                        GateDownTimes = item.Where(w => w.GetType().Name == nameof(TimeToGateDownValue)).Cast<TimeToGateDownValue>().ToList(),
                        CallMaxOutTimes = item.Where(w => w.GetType().Name == nameof(TimeToCallMaxOutValue)).Cast<TimeToCallMaxOutValue>().ToList()
                    });
                }
            }

            return Task.FromResult<IEnumerable<PreemptDetailResult>>(result);
        }
    }
}
