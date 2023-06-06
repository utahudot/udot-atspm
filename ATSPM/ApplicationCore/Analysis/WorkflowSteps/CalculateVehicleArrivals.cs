using ATSPM.Application.Analysis.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class CalculateVehicleArrivals : TransformProcessStepBase<Tuple<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>, IReadOnlyList<ICycleArrivals>>
    {
        public CalculateVehicleArrivals(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IReadOnlyList<ICycleArrivals>> Process(Tuple<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>> input, CancellationToken cancelToken = default)
        {
            var result = input.Item2.Select(s => new CycleArrivals(s)
            {
                Vehicles = input.Item1.Where(w => w.Detector.Approach?.Signal?.SignalId == s.SignalId && s.InRange(w.CorrectedTimeStamp))
                .Select(v => new Vehicle(v, s))
                .ToList()
            }).ToList();

            return Task.FromResult<IReadOnlyList<ICycleArrivals>>(result);
        }
    }
}
