using ATSPM.Application.Analysis.Common;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using ATSPM.Domain.Extensions;
using ATSPM.Domain.Workflows;
using Microsoft.VisualBasic;
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
            var logFilter = input.Item2?.Where(w => w.SignalIdentifier == input.Item1?.Signal.SignalIdentifier && input.Item1.Detectors.Select(s => s.DetectorChannel).Contains(w.DetectorChannel)).ToList();

            if (logFilter == null || logFilter.Count == 0)
                return Task.FromResult(Tuple.Create<Approach, Volumes>(input.Item1, null));

            var result = Tuple.Create(input.Item1, new Volumes(new TimelineOptions()
            {
                Start = logFilter?.Min(m => m.CorrectedTimeStamp).RoundDown(TimeSpan.FromMinutes(15)) ?? DateTime.MinValue,
                End = logFilter?.Max(m => m.CorrectedTimeStamp).RoundUp(TimeSpan.FromMinutes(15)) ?? DateTime.MaxValue,
                Type = TimelineType.Minutes,
                Size = 15
            }));

            result.Item2.ForEach(f =>
            {
                f.Phase = input.Item1?.ProtectedPhaseNumber ?? 0;
                f.Direction = input.Item1.DirectionTypeId;
                f.AddRange(logFilter.Where(w => f.InRange(w.CorrectedTimeStamp)));

            });

            return Task.FromResult(result);
        }
    }
}
