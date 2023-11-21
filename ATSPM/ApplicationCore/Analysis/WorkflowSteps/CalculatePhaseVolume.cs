using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Extensions;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using ATSPM.Domain.Extensions;
using ATSPM.Domain.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class CalculatePhaseVolume : TransformProcessStepBase<Tuple<Approach, IEnumerable<CorrectedDetectorEvent>>, Tuple<Approach, Volumes>>
    {
        public CalculatePhaseVolume(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<Tuple<Approach, Volumes>> Process(Tuple<Approach, IEnumerable<CorrectedDetectorEvent>> input, CancellationToken cancelToken = default)
        {
            var eventFilter = input.FilterCorrectedDetectorEvents();

            if (eventFilter == null || eventFilter.Count == 0)
                return Task.FromResult(Tuple.Create<Approach, Volumes>(input.Item1, null));

            var result = Tuple.Create(input.Item1, new Volumes(eventFilter.CreateTimeline<Volume>(TimelineType.Minutes, 15))
            {
                SignalIdentifier = input.Item1.Signal.SignalIdentifier,
                PhaseNumber = input.Item1.ProtectedPhaseNumber,
                Direction = input.Item1.DirectionTypeId
            });

            result.Item2.ForEach((f =>
            {
                f.SignalIdentifier = input.Item1?.Signal.SignalIdentifier;
                f.PhaseNumber = input.Item1?.ProtectedPhaseNumber ?? 0;
                f.Direction = input.Item1.DirectionTypeId;
                f.AddRange(eventFilter.Where(w => f.InRange(w)));
            }));

            return Task.FromResult(result);
        }
    }
}
