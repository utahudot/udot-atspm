﻿using ATSPM.Data.Enums;
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
    public class AggregatePriorityCodes : TransformProcessStepBase<Tuple<Signal, int, IEnumerable<ControllerEventLog>>, IEnumerable<PriorityAggregation>>
    {
        public AggregatePriorityCodes(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<PriorityAggregation>> Process(Tuple<Signal, int, IEnumerable<ControllerEventLog>> input, CancellationToken cancelToken = default)
        {
            var signal = input.Item1;
            var priority = input.Item2;
            var logs = input.Item3.FromSpecification(new ControllerLogSignalAndParamterFilterSpecification(signal, priority));

            var tl = new Timeline<PriorityAggregation>(logs, TimeSpan.FromMinutes(15));

            tl.Segments.ToList().ForEach(f =>
            {
                f.SignalIdentifier = input.Item1.SignalIdentifier;
                f.PriorityNumber = priority;
                f.PriorityRequests = logs.Count(c => c.EventCode == (int)DataLoggerEnum.TSPCheckIn && f.InRange(c));
                f.PriorityServiceEarlyGreen = logs.Count(c => c.EventCode == (int)DataLoggerEnum.TSPAdjustmenttoEarlyGreen && f.InRange(c));
                f.PriorityServiceExtendedGreen = logs.Count(c => c.EventCode == (int)DataLoggerEnum.TSPAdjustmenttoExtendGreen && f.InRange(c));
            });

            var result = tl.Segments.AsEnumerable();

            return Task.FromResult(result);
        }
    }
}