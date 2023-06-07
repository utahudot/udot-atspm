using ATSPM.Application.Analysis.ApproachDelay;
using ATSPM.Application.Analysis.Common;
using ATSPM.Data.Models;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class GenerateApproachDelayResults : TransformManyProcessStepBase<IReadOnlyList<Vehicle>, ApproachDelayResult>
    {
        public GenerateApproachDelayResults(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<ApproachDelayResult>> Process(IReadOnlyList<Vehicle> input, CancellationToken cancelToken = default)
        {
            var result = new List<ApproachDelayResult>();

            foreach (var signal in input.GroupBy(g => g.SignalIdentifier))
            {
                foreach (var phase in signal.GroupBy(g => g.PhaseNumber))
                {
                    foreach (var vehicles in phase.GroupBy(g => g.DetChannel))
                    {
                        result.Add(new ApproachDelayResult()
                        {
                            Start = vehicles.Min(m => m.Start),
                            End = vehicles.Max(m => m.End),
                            SignalId = signal.Key,
                            Phase = phase.Key,
                            AverageDelay = vehicles.Average(a => a.Delay),
                            TotalDelay = vehicles.Sum(s => s.Delay) / 3600
                        });
                    }
                }
            }

            //var result = input
            //    .SelectMany(m => m.Vehicles)
            //    .GroupBy(g => g.SignalIdentifier, (s, x) =>
            //    x.GroupBy(g => g.PhaseNumber, (p, y) =>
            //    y.GroupBy(g => g.DetChannel, (d, z) =>

            //    new ApproachDelayResult()
            //    {
            //        Start = z.Min(m => m.Start),
            //        End = z.Max(m => m.End),
            //        SignalId = s,
            //        Phase = p,
            //        AverageDelay = z.Average(a => a.Delay),
            //        TotalDelay = z.Sum(s => s.Delay) / 3600
            //    }))
            //    .SelectMany(m => m))
            //    .SelectMany(m => m);

            return Task.FromResult<IEnumerable<ApproachDelayResult>>(result);
        }
    }
}
