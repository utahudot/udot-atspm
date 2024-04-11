using ATSPM.Application.Analysis.Common;
using ATSPM.Data.Enums;
using ATSPM.Data.Interfaces;
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
    public class IdentifyTerminationTypesAndTimes : TransformProcessStepBase<Tuple<Approach, int, IEnumerable<ControllerEventLog>>, Tuple<Approach, int, PhaseTerminations>>
    {
        private readonly int _consecutiveCounts;

        public IdentifyTerminationTypesAndTimes(int consecutiveCounts = 3, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) 
        {
            _consecutiveCounts = consecutiveCounts;
        }

        protected override Task<Tuple<Approach, int, PhaseTerminations>> Process(Tuple<Approach, int, IEnumerable<ControllerEventLog>> input, CancellationToken cancelToken = default)
        {
            var filters = new List<int>()
            {
                (int)IndianaEnumerations.PhaseGapOut,
                (int)IndianaEnumerations.PhaseMaxOut,
                (int)IndianaEnumerations.PhaseForceOff,
                (int)IndianaEnumerations.PhaseGreenTermination
            };
            
            var approach = input.Item1;
            var phase = input.Item2;
            var logs = input.Item3
                .Where(w => w.EventParam == phase)
                .Where(w => filters.Contains(w.EventCode))
                .OrderBy(o => o.Timestamp).ToList();

            //if there are two consecutive )IndianaEnumerations.PhaseGreenTermination then the second denotes an unknown termination
            var consecGreenTerminations = logs.GetLastConsecutiveEvent(2).Where(w => w.EventCode == (int)IndianaEnumerations.PhaseGreenTermination).ToList();

            //remove IndianaEnumerations.PhaseGreenTermination and get the consecutive terminations
            var consecTerminations = logs.Where(r => r.EventCode != (int)IndianaEnumerations.PhaseGreenTermination).GetLastConsecutiveEvent(_consecutiveCounts).ToList();

            var stuff = new PhaseTerminations(consecTerminations.Union(consecGreenTerminations))
            {
                LocationIdentifier = approach.Location.LocationIdentifier,
                PhaseNumber = phase,
            };

            var result = Tuple.Create(approach, phase, stuff);

            return Task.FromResult(result);
        }
    }
}
