﻿using ATSPM.Application.Analysis.Common;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Workflows;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    /// <summary>
    /// Creates a list of <see cref="RedToRedCycle"/>
    /// <list type="number">
    /// <listheader>Steps to create the <see cref="RedToRedCycle"/></listheader>
    /// 
    /// <item>
    /// <term>Identify the Beginning of Each Cycle</term>
    /// <description>
    /// The beginning of the cycle
    /// for a given phase is defined as the end of <see cref="DataLoggerEnum.PhaseEndYellowChange"/>. The
    /// event log is queried to find the records where the Event Code is 9. Each instance
    /// of <see cref="DataLoggerEnum.PhaseEndYellowChange"/> is indicated as the start of the cycle.
    /// </description>
    /// </item>
    /// 
    /// <item>
    /// <term>Identify the Change to Green for Each Cycle</term>
    /// <description>
    /// During this step, the event log is queried to find the records where the Event Code <see cref="DataLoggerEnum.PhaseBeginGreen"/>.
    /// The duration from the beginning of the cycle to when the given phasechanges to green(total red interval)
    /// is calculated in reference to the first redevent (begin) of the cycle
    /// </description>
    /// </item>
    /// 
    /// <item>
    /// <term>Identify the Change to Yellow for Each Cycle</term>
    /// <description>
    /// During this step, the event log is queried to find the record where the Event Code <see cref="DataLoggerEnum.PhaseBeginYellowChange"/>.
    /// The duration from the beginning of the cycle to when the given phase
    /// changes to yellow(total green interval) is calculated in reference to the first red event (begin) of the cycle
    /// </description>
    /// </item>
    /// 
    /// <item>
    /// <term>Identify the Change to Red at the End of Each Cycle</term>
    /// <description>
    /// During this step, the event log is queried to find the records where the Event Code <see cref="DataLoggerEnum.PhaseEndYellowChange"/>. 
    /// The duration from the beginning of the cycle to when the given phase changes to red(yellow clearance interval)
    /// is calculated in reference to the firstred event (begin) of the cycle
    /// </description>
    /// </item>
    /// 
    /// </list>
    /// </summary>
    public class CreateRedToRedCycles : TransformProcessStepBase<IEnumerable<ControllerEventLog>, IReadOnlyList<RedToRedCycle>>
    {
        /// <inheritdoc/>
        public CreateRedToRedCycles(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        /// <inheritdoc/>
        protected override Task<IReadOnlyList<RedToRedCycle>> Process(IEnumerable<ControllerEventLog> input, CancellationToken cancelToken = default)
        {
            var result = input.Where(l => l.EventCode == 1 || l.EventCode == 8 || l.EventCode == 9)
                .OrderBy(o => o.Timestamp)
                .GroupBy(g => g.SignalIdentifier, (s, x) => x
                .GroupBy(g => g.EventParam, (p, y) => y    
                .Where((w, i) => y.Count() > 3 && i <= y.Count() - 3)
                .Where((w, i) => w.EventCode == 9 && y.ElementAt(i + 1).EventCode == 1 && y.ElementAt(i + 2).EventCode == 8 && y.ElementAt(i + 3).EventCode == 9)
                .Select((s, i) => y.Skip(y.ToList().IndexOf(s)).Take(4))
                .Select(m => new RedToRedCycle()
                {
                    Start = m.ElementAt(0).Timestamp,
                    End = m.ElementAt(3).Timestamp,
                    GreenEvent = m.ElementAt(1).Timestamp,
                    YellowEvent = m.ElementAt(2).Timestamp,
                    PhaseNumber = p,
                    SignalIdentifier = s
                }))
                .SelectMany(m => m))
                .SelectMany(m => m)
                .ToList();

            return Task.FromResult<IReadOnlyList<RedToRedCycle>>(result);
        }
    }
}