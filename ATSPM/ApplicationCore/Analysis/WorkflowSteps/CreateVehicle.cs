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
    public class CreateVehicle : TransformProcessStepBase<Tuple<IEnumerable<Tuple<Detector, IEnumerable<CorrectedDetectorEvent>>>, Tuple<Approach, IEnumerable<RedToRedCycle>>>, Tuple<Approach, IEnumerable<Vehicle>>>
    {
        public CreateVehicle(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<Tuple<Approach, IEnumerable<Vehicle>>> Process(Tuple<IEnumerable<Tuple<Detector, IEnumerable<CorrectedDetectorEvent>>>, Tuple<Approach, IEnumerable<RedToRedCycle>>> input, CancellationToken cancelToken = default)
        {
            var result = Tuple.Create(input.Item2?.Item1,
                input.Item2?.Item2?.Select(s =>
                input.Item1.Where(w => w.Item1.ApproachId == input.Item2.Item1.Id)
                .SelectMany(m => m.Item2?.Where(w => w.SignalIdentifier == input.Item2?.Item1?.Signal.SignalIdentifier && s.InRange(w.Timestamp)))
                .Select(v => new Vehicle()
                {
                    SignalIdentifier = v.SignalIdentifier,
                    Timestamp = v.Timestamp,
                    DetectorChannel = v.DetectorChannel,
                    PhaseNumber = s.PhaseNumber,
                    Start = s.Start,
                    End = s.End,
                    YellowEvent = s.YellowEvent,
                    GreenEvent = s.GreenEvent,
                })).SelectMany(m => m));

            return Task.FromResult(result);
        }
    }
}
