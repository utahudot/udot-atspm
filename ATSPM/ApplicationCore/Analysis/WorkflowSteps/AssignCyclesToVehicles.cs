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
    public class AssignCyclesToVehicles : TransformProcessStepBase<Tuple<IEnumerable<Tuple<Detector, IEnumerable<CorrectedDetectorEvent>>>, Tuple<Approach, IEnumerable<RedToRedCycle>>>, Tuple<Approach, IEnumerable<Vehicle>>>
    {
        public AssignCyclesToVehicles(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<Tuple<Approach, IEnumerable<Vehicle>>> Process(Tuple<IEnumerable<Tuple<Detector, IEnumerable<CorrectedDetectorEvent>>>, Tuple<Approach, IEnumerable<RedToRedCycle>>> input, CancellationToken cancelToken = default)
        {
            var de = input.Item1.Where(w => w.Item1.ApproachId == input.Item2.Item1.Id);
            
            var result = Tuple.Create(input.Item2.Item1, input.Item2.Item2.Select(s => de.SelectMany(m => m.Item2.Where(w => s.InRange(w.CorrectedTimeStamp))).Select(v => new Vehicle(v, s))).SelectMany(m => m));
            
            //var result = new List<Vehicle>();

            //foreach (var v in input.Item1)
            //{
            //    //TODO: Add phase validation here too!!!
            //    var redCycle = input.Item2?.FirstOrDefault(w => w.SignalIdentifier == v.Detector.Approach?.Signal?.SignalIdentifier && w.InRange(v.CorrectedTimeStamp));

            //    if (redCycle != null)
            //    {
            //        result.Add(new Vehicle(v, redCycle));
            //    }
            //}

            return Task.FromResult(result);
        }
    }
}
