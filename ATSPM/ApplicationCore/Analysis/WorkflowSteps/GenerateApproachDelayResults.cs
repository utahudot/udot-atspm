using ATSPM.Application.Analysis.ApproachDelay;
using ATSPM.Application.Analysis.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class GenerateApproachDelayResults : TransformProcessStepBase<IEnumerable<Vehicle>, IEnumerable<ApproachDelayResult>>
    {
        public GenerateApproachDelayResults(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<ApproachDelayResult>> Process(IEnumerable<Vehicle> input, CancellationToken cancelToken = default)
        {
            var result = new List<ApproachDelayResult>();

            foreach (var signal in input.GroupBy(g => g.SignalId))
            {
                foreach (var phase in signal.GroupBy(g => g.Phase))
                {
                    foreach (var vehicles in phase.GroupBy(g => g.DetChannel))
                    {
                        result.Add(new ApproachDelayResult()
                        {
                            Start = vehicles.Min(m => m.StartTime),
                            End = vehicles.Max(m => m.EndTime),
                            SignalId = signal.Key,
                            Phase = phase.Key,
                            Vehicles = vehicles.ToList()
                        });
                    }
                }
            }

            return Task.FromResult<IEnumerable<ApproachDelayResult>>(result);
        }
    }
}
