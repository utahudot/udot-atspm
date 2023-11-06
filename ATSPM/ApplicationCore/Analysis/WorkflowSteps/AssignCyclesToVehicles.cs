using ATSPM.Application.Analysis.Common;
using ATSPM.Domain.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class AssignCyclesToVehicles : TransformProcessStepBase<Tuple<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>, IReadOnlyList<Vehicle>>
    {
        public AssignCyclesToVehicles(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IReadOnlyList<Vehicle>> Process(Tuple<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>> input, CancellationToken cancelToken = default)
        {
            var result = new List<Vehicle>();

            foreach (var v in input.Item1)
            {
                //TODO: Add phase validation here too!!!
                var redCycle = input.Item2?.FirstOrDefault(w => w.SignalIdentifier == v.Detector.Approach?.Signal?.SignalIdentifier && w.InRange(v.CorrectedTimeStamp));

                if (redCycle != null)
                {
                    result.Add(new Vehicle(v, redCycle));
                }
            }

            return Task.FromResult<IReadOnlyList<Vehicle>>(result);
        }
    }
}
