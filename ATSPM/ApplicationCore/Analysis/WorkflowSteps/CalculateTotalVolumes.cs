using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Common;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using ATSPM.Domain.Extensions;
using ATSPM.Domain.Workflows;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ATSPM.Application.Extensions;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class CalculateTotalVolumes : TransformProcessStepBase<Tuple<Tuple<Approach, Volumes>, Tuple<Approach, Volumes>>, Tuple<Approach, TotalVolumes>>
    {
        protected override Task<Tuple<Approach, TotalVolumes>> Process(Tuple<Tuple<Approach, Volumes>, Tuple<Approach, Volumes>> input, CancellationToken cancelToken = default)
        {
            //var primaryFilter = input.Item1.Item2.Where(w => w.SignalIdentifier == input.Item1?.Signal.SignalIdentifier && input.Item1.Detectors.Select(s => s.DetectorChannel).Contains(w.DetectorChannel)).ToList();

            var p = input.Item1.Item1;
            var o = input.Item2.Item1;
            
            if (p.DirectionTypeId == new OpposingDirection(o.DirectionTypeId) && p.Signal.SignalIdentifier == o.Signal.SignalIdentifier)
            {
                var volumes = input.Item1.Item2.Union(input.Item2.Item2);

                var result = new TotalVolumes(volumes.CreateTimeline<TotalVolume>(TimelineType.Minutes, 15))
                {
                    SignalIdentifier = p.Signal.SignalIdentifier,
                };

                result.ForEach(f =>
                {
                    f.Primary = input.Item1.Item2.FirstOrDefault(d => f.InRange(d));
                    f.Opposing = input.Item2.Item2.FirstOrDefault(d => f.InRange(d));
                });

                return Task.FromResult(Tuple.Create(input.Item1.Item1, result));
            }

            return Task.FromResult(Tuple.Create<Approach, TotalVolumes>(null, null));
        }
    }
}
