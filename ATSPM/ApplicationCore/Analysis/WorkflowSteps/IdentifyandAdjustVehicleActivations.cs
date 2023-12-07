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
    public class IdentifyandAdjustVehicleActivations : TransformProcessStepBase<Tuple<Approach, IEnumerable<ControllerEventLog>>, Tuple<Approach, IEnumerable<CorrectedDetectorEvent>>>
    {
        /// <inheritdoc/>
        public IdentifyandAdjustVehicleActivations(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        /// <inheritdoc/>
        protected override Task<Tuple<Approach, IEnumerable<CorrectedDetectorEvent>>> Process(Tuple<Approach, IEnumerable<ControllerEventLog>> input, CancellationToken cancelToken = default)
        {
            var result = Tuple.Create(input.Item1, input.Item1?.Detectors.GroupJoin(input.Item2, o => o.DetectorChannel, i => i.EventParam, (o, i) =>
            i.Where(w => w.SignalIdentifier == input.Item1?.Signal?.SignalIdentifier && w.EventCode == (int)DataLoggerEnum.DetectorOn)
            .Select(s => new CorrectedDetectorEvent()
            {
                SignalIdentifier = s.SignalIdentifier,
                PhaseNumber = o.Approach.ProtectedPhaseNumber,
                Direction = o.Approach.DirectionTypeId,
                DetectorChannel = o.DetectorChannel,
                Timestamp = AtspmMath.AdjustTimeStamp(s.Timestamp, input.Item1?.Mph ?? 0, o?.DistanceFromStopBar ?? 0, o?.LatencyCorrection ?? 0)
            }))
            .SelectMany(m => m));

            return Task.FromResult(result);
        }
    }
}