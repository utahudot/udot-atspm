using ATSPM.Application.Analysis.Common;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class IdentifyandAdjustVehicleActivations : TransformProcessStepBase<IEnumerable<Tuple<Detector, IEnumerable<ControllerEventLog>>>, IEnumerable<CorrectedDetectorEvent>>
    {
        public IdentifyandAdjustVehicleActivations(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<CorrectedDetectorEvent>> Process(IEnumerable<Tuple<Detector, IEnumerable<ControllerEventLog>>> input, CancellationToken cancelToken = default)
        {
            var result = input
                .Select(s =>
                Tuple.Create(s.Item1, s.Item2.Where(w => w.EventCode == (int)DataLoggerEnum.DetectorOn && s.Item1.Approach?.Signal?.SignalId == w.SignalId && w.EventParam == s.Item1.DetChannel)))
                .Select(s => s
                .Item2.Select(c =>
                new CorrectedDetectorEvent(s.Item1)
                {
                    CorrectedTimeStamp = AtspmMath.AdjustTimeStamp(c.Timestamp, s.Item1?.Approach?.Mph ?? 0, s.Item1.DistanceFromStopBar ?? 0, s.Item1.LatencyCorrection)
                }))
                .SelectMany(s => s);

            return Task.FromResult(result);
        }
    }
}
