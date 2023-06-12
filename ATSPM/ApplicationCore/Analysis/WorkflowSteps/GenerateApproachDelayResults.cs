using ATSPM.Application.Analysis.ApproachDelay;
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.Plans;
using ATSPM.Application.Analysis.PurdueCoordination;
using ATSPM.Data.Interfaces;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class GenerateApproachDelayResults : TransformProcessStepBase<Tuple<IReadOnlyList<Vehicle>, IReadOnlyList<ApproachDelayPlan>>, ApproachDelayResult>
    {
        public GenerateApproachDelayResults(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<ApproachDelayResult> Process(Tuple<IReadOnlyList<Vehicle>, IReadOnlyList<ApproachDelayPlan>> input, CancellationToken cancelToken = default)
        {
            List<ApproachDelayPlan> plans;

            if (input.Item2.Count == 0)
            {
                plans = input.Item1.GroupBy(g => g.SignalIdentifier, (s, i) => new ApproachDelayPlan()
                {
                    SignalIdentifier = s,
                    Start = i.Min(m => m.Start),
                    End = i.Max(m => m.End)
                }).ToList();
            }
            else
            {
                plans = input.Item2.ToList();
            }

            foreach (var p in plans)
            {
                
                
                foreach (var r in input.Item1)
                {
                    p.TryAssignToPlan(r);
                }

                Console.Write($"plan: {p} - {p.Vehicles?.Count}\n");
            }

            var result = new ApproachDelayResult() 
            { 
                Start = plans.SelectMany(m => m.Vehicles).Min(m => m.Start),
                End = plans.SelectMany(m => m.Vehicles).Max(m => m.End),
                Plans = plans.Where(w => w.Vehicles.Count > 0).ToList() 
            };

            return Task.FromResult(result);
        }
    }
}
