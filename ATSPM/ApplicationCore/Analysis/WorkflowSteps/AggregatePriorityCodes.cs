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
    public class AggregatePriorityCodes : TransformManyProcessStepBase<Tuple<Signal, IEnumerable<ControllerEventLog>>, IEnumerable<PriorityAggregation>>
    {
        public AggregatePriorityCodes(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<IEnumerable<PriorityAggregation>>> Process(Tuple<Signal, IEnumerable<ControllerEventLog>> input, CancellationToken cancelToken = default)
        {
            var result = input.Item2
                .FromSpecification(new ControllerLogSignalFilterSpecification(input.Item1))
                .GroupBy(g => g.EventParam, (priority, i) =>
                {
                    var tl = new Timeline<PriorityAggregation>(i, TimeSpan.FromMinutes(15));

                    tl.Segments.ToList().ForEach(f =>
                    {
                        f.SignalIdentifier = input.Item1.SignalIdentifier;
                        f.PriorityNumber = priority;
                        f.PriorityRequests = i.Count(c => c.EventCode == (int)DataLoggerEnum.TSPCheckIn && f.InRange(c));
                        f.PriorityServiceEarlyGreen = i.Count(c => c.EventCode == (int)DataLoggerEnum.TSPAdjustmenttoEarlyGreen && f.InRange(c));
                        f.PriorityServiceExtendedGreen = i.Count(c => c.EventCode == (int)DataLoggerEnum.TSPAdjustmenttoExtendGreen && f.InRange(c));
                    });

                    //TODO: this is for testing
                    return tl.Segments.Where(w => i.Any(a => w.InRange(a))).AsEnumerable();

                    //return tl.Segments.AsEnumerable();
                });

            return Task.FromResult(result);
        }
    }
}
