using ATSPM.Application.Analysis.ApproachDelay;
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.Plans;
using ATSPM.Application.Analysis.PurdueCoordination;
using ATSPM.Data.Interfaces;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using ATSPM.Domain.Workflows;
using System;
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
            var result = input.GroupBy(g => g.SignalIdentifier, (signal, x) =>
            x.GroupBy(g => g.PhaseNumber, (phase, y) =>
            y.GroupBy(g => g.DetChannel, (det, z) => new ApproachDelayResult()
            {
                SignalIdentifier = signal,
                PhaseNumber = phase,
                Start = z.Min(m => m.Start),
                End = z.Max(m => m.End),
                Plans = new List<ApproachDelayPlan>() {
                    new ApproachDelayPlan()
                    {
                        SignalIdentifier = signal,
                        Start = z.Min(m => m.Start),
                        End = z.Max(m => m.End),
                        Vehicles = z.ToList()
                    }
                }
            })).SelectMany(m => m))
                .SelectMany(m => m);

            //var plans = new List<ApproachDelayPlan>();

            //var result = input.GroupBy(g => g.SignalIdentifier, (signal, x) =>
            //x.GroupBy(g => g.PhaseNumber, (phase, y) =>
            //y.GroupBy(g => g.DetChannel, (det, z) => new ApproachDelayResult()
            //{
            //    SignalIdentifier = signal,
            //    PhaseNumber = phase,
            //    Start = z.Min(m => m.Start),
            //    End = z.Max(m => m.End),
            //    Plans = plans.Where(w => w.SignalIdentifier == signal)
            //    .Select(p => new ApproachDelayPlan()
            //    {
            //        SignalIdentifier = p.SignalIdentifier,
            //        PlanNumber = p.PlanNumber,
            //        Start = p.Start,
            //        End = p.End,
            //        Vehicles = z.ToList()
            //    }).Where(w => w.Vehicles.Count > 0)
            //    .ToList()
            //})).SelectMany(m => m))
            //    .SelectMany(m => m)
            //    .ToList();

            return Task.FromResult(result);
        }
    }
}
