using ATSPM.Application.Analysis.Common;
using ATSPM.Data.Enums;
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
    /// <summary>
    /// <c>Identify and Adjust Vehicle Actuations</c>
    /// During this step, the event log is queried to find detector activations for the subject phase
    /// (the records where the Event Code is <see cref="DataLoggerEnum.DetectorOn"/> and Event Parameter is a detector channel assigned to the subject phase). 
    /// The timestamps of the EC 82 events are noted.
    /// Timestamps for detector on events may need to be adjusted to represent vehicle arrivals at the stop bar
    /// rather than at the detector location or toadjust based on possible detector latency differences.
    /// </summary>
    public class IdentifyandAdjustVehicleActivations : TransformProcessStepBase<IEnumerable<Tuple<Detector, IEnumerable<ControllerEventLog>>>, IReadOnlyList<CorrectedDetectorEvent>>
    {
        /// <inheritdoc/>
        public IdentifyandAdjustVehicleActivations(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        /// <inheritdoc/>
        protected override Task<IReadOnlyList<CorrectedDetectorEvent>> Process(IEnumerable<Tuple<Detector, IEnumerable<ControllerEventLog>>> input, CancellationToken cancelToken = default)
        {
            var result = input
                .Select(s =>
                Tuple.Create(s.Item1, s.Item2.Where(w => w.EventCode == (int)DataLoggerEnum.DetectorOn && s.Item1.Approach?.Signal?.SignalIdentifier == w.SignalIdentifier && w.EventParam == s.Item1.DetChannel)))
                .Select(s => s
                .Item2.Select(c =>
                new CorrectedDetectorEvent(s.Item1)
                {
                    CorrectedTimeStamp = AtspmMath.AdjustTimeStamp(c.TimeStamp, s.Item1?.Approach?.Mph ?? 0, s.Item1.DistanceFromStopBar ?? 0, s.Item1.LatencyCorrection)
                }))
            .SelectMany(s => s)
            .ToList();

            return Task.FromResult<IReadOnlyList<CorrectedDetectorEvent>>(result);
        }
    }
}
