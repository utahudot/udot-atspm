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
    public class GroupDetectorsByDetectorEvent : TransformManyProcessStepBase<Tuple<Approach, IEnumerable<ControllerEventLog>>, Tuple<Detector, int, IEnumerable<ControllerEventLog>>>
    {
        /// <inheritdoc/>
        public GroupDetectorsByDetectorEvent(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        /// <inheritdoc/>
        protected override Task<IEnumerable<Tuple<Detector, int, IEnumerable<ControllerEventLog>>>> Process(Tuple<Approach, IEnumerable<ControllerEventLog>> input, CancellationToken cancelToken = default)
        {
            var approach = input.Item1;
            var logs = input.Item2;

            var result = approach.Detectors
                .GroupJoin(logs
                .Where(w => w.SignalIdentifier == approach?.Signal?.SignalIdentifier)
                .Where(w => w.EventCode == (int)DataLoggerEnum.DetectorOn), 
                o => o.DetectorChannel, i => i.EventParam, (o, i) => Tuple.Create(o, o.DetectorChannel, i));

            return Task.FromResult(result);
        }
    }
}
