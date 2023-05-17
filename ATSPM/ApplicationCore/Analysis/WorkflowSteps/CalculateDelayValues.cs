using ATSPM.Application.Analysis.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class CalculateDelayValues : TransformProcessStepBase<Tuple<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>, IEnumerable<Vehicle>>
    {
        public CalculateDelayValues(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<Vehicle>> Process(Tuple<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>> input, CancellationToken cancelToken = default)
        {
            var result = new List<Vehicle>();

            foreach (var v in input.Item1)
            {
                var redCycle = input.Item2?.FirstOrDefault(w => w.SignalId == v.SignalId && v.TimeStamp >= w.StartTime && v.TimeStamp <= w.EndTime);

                if (redCycle != null)
                {
                    result.Add(new Vehicle(v, redCycle));
                }
            }

            return Task.FromResult<IEnumerable<Vehicle>>(result);
        }
    }
}
