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
using ATSPM.Domain.Extensions;
using ATSPM.Application.Specifications;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class AggregatePreemptCodes : TransformProcessStepBase<Tuple<Location, int, IEnumerable<ControllerEventLog>>, IEnumerable<PreemptionAggregation>>
    {
        public AggregatePreemptCodes(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<PreemptionAggregation>> Process(Tuple<Location, int, IEnumerable<ControllerEventLog>> input, CancellationToken cancelToken = default)
        {
            var signal = input.Item1;
            var preempt = input.Item2;
            var logs = input.Item3.FromSpecification(new ControllerLogSignalAndParamterFilterSpecification(signal, preempt));

            var tl = new Timeline<PreemptionAggregation>(logs, TimeSpan.FromMinutes(15));

            tl.Segments.ToList().ForEach(f =>
            {
                f.LocationIdentifier = signal.LocationIdentifier;
                f.PreemptNumber = preempt;
                f.PreemptServices = logs.Count(c => c.EventCode == (int)DataLoggerEnum.PreemptCallInputOn && f.InRange(c));
                f.PreemptServices = logs.Count(c => c.EventCode == (int)DataLoggerEnum.PreemptEntryStarted && f.InRange(c));
            });

            var result = tl.Segments.AsEnumerable();

            return Task.FromResult(result);
        }
    }
}
