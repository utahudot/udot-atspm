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
    public class GenerateApproachDelayResults : TransformProcessStepBase<Tuple<Approach, IEnumerable<Vehicle>>, Tuple<Approach, ApproachDelayResult>>
    {
        public GenerateApproachDelayResults(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<Tuple<Approach, ApproachDelayResult>> Process(Tuple<Approach, IEnumerable<Vehicle>> input, CancellationToken cancelToken = default)
        {
            var result = Tuple.Create(input.Item1, new ApproachDelayResult());
            
            //var result = input.Item2.GroupBy(g => g.SignalIdentifier, (signal, x) =>
            //x.GroupBy(g => g.PhaseNumber, (phase, y) =>
            //y.GroupBy(g => g.DetectorChannel, (det, z) => new ApproachDelayResult()
            //{
            //    SignalIdentifier = signal,
            //    PhaseNumber = phase,
            //    Start = z.Min(m => m.Start),
            //    End = z.Max(m => m.End),
            //    Plans = new List<ApproachDelayPlan>() {
            //        new ApproachDelayPlan()
            //        {
            //            SignalIdentifier = signal,
            //            Start = z.Min(m => m.Start),
            //            End = z.Max(m => m.End),
            //            Vehicles = z.ToList()
            //        }
            //    }
            //})).SelectMany(m => m))
            //    .SelectMany(m => m);

            return Task.FromResult(result);
        }
    }
}
