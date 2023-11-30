using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using ATSPM.Domain.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class AggregateDetectorEvents : TransformProcessStepBase<Tuple<Detector, int, IEnumerable<ControllerEventLog>>, IEnumerable<DetectorEventCountAggregation>>
    {
        /// <inheritdoc/>
        public AggregateDetectorEvents(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        /// <inheritdoc/>
        protected override Task<IEnumerable<DetectorEventCountAggregation>> Process(Tuple<Detector, int, IEnumerable<ControllerEventLog>> input, CancellationToken cancelToken = default)
        {
            var detector = input.Item1;
            var detectorChannel = input.Item2;
            var logs = input.Item3
                .Where(w => w.SignalIdentifier == detector.Approach?.Signal?.SignalIdentifier)
                .Where(w => w.EventCode == (int)DataLoggerEnum.DetectorOn)
                .Where(w => w.EventParam == detectorChannel);

            var tl = new Timeline<DetectorEventCountAggregation>(logs, TimeSpan.FromMinutes(15));

            tl.Segments.ToList().ForEach(f =>
            {
                f.SignalIdentifier = detector.Approach?.Signal?.SignalIdentifier;
                f.ApproachId = detector.ApproachId;
                f.DetectorPrimaryId = detector.Id;
                f.EventCount = logs.Count(w => f.InRange(w));
            });

            //TODO: this is for testing
            var result = tl.Segments.AsEnumerable();

            //var result = tl.Segments.AsEnumerable();

            return Task.FromResult(result);
        }
    }
}
