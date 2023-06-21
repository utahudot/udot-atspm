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
    /// <summary>
    /// Creates a list of <see cref="CycleArrivals"/>
    /// <list type="number">
    /// <listheader>Steps to create the <see cref="CycleArrivals"/></listheader>
    /// 
    /// <item>
    /// Identify <see cref="CorrectedDetectorEvent"/> arriving durring the green stage of <see cref="RedToRedCycle"/>
    /// </item>
    /// 
    /// <item>
    /// Identify <see cref="CorrectedDetectorEvent"/> arriving durring the yellow stage of <see cref="RedToRedCycle"/>
    /// </item>
    /// 
    /// <item>
    /// Identify <see cref="CorrectedDetectorEvent"/> arriving durring the red stage of <see cref="RedToRedCycle"/>
    /// </item>
    /// 
    /// </list>
    /// </summary>
    public class CalculateVehicleArrivals : TransformProcessStepBase<Tuple<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>, IReadOnlyList<CycleArrivals>>
    {
        /// <inheritdoc/>
        public CalculateVehicleArrivals(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        /// <inheritdoc/>
        protected override Task<IReadOnlyList<CycleArrivals>> Process(Tuple<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>> input, CancellationToken cancelToken = default)
        {
            var result = input.Item2.Select(s => new CycleArrivals(s)
            {
                Vehicles = input.Item1.Where(w => w.Detector.Approach?.Signal?.SignalId == s.SignalIdentifier && s.InRange(w.CorrectedTimeStamp))
                .Select(v => new Vehicle(v, s))
                .ToList()
            }).ToList();

            return Task.FromResult<IReadOnlyList<CycleArrivals>>(result);
        }
    }
}
