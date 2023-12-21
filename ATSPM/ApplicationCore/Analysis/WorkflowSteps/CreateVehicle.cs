using ATSPM.Application.Analysis.Common;
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
    public class CreateVehicle : TransformProcessStepBase<Tuple<Tuple<Approach, IEnumerable<CorrectedDetectorEvent>>, Tuple<Approach, IEnumerable<RedToRedCycle>>>, Tuple<Approach, IEnumerable<Vehicle>>>
    {
        public CreateVehicle(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<Tuple<Approach, IEnumerable<Vehicle>>> Process(Tuple<Tuple<Approach, IEnumerable<CorrectedDetectorEvent>>, Tuple<Approach, IEnumerable<RedToRedCycle>>> input, CancellationToken cancelToken = default)
        {
            var result = input.Item2.Item2?.GroupJoin(input.Item1.Item2, 
                o => new { o.LocationIdentifier, o.PhaseNumber }, 
                i => new { i.LocationIdentifier, i.PhaseNumber },
                (o, i) => i.Where(w => o.InRange(w.Timestamp))
                .Select(s => new Vehicle(s, o)))
                .SelectMany(m => m);

            return Task.FromResult(Tuple.Create(input.Item1.Item1, result));
        }
    }
}
