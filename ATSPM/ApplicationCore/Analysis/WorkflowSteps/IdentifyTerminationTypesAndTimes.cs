using ATSPM.Application.Analysis.Common;
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
    public class Phase
    {
        //public Phase(
        //    int phaseNumber,
        //    ICollection<DateTime> gapOuts,
        //    ICollection<DateTime> maxOuts,
        //    ICollection<DateTime> forceOffs,
        //    ICollection<DateTime> pedWalkBegins,
        //    ICollection<DateTime> unknownTerminations)
        //{
        //    PhaseNumber = phaseNumber;
        //    GapOuts = gapOuts;
        //    MaxOuts = maxOuts;
        //    ForceOffs = forceOffs;
        //    PedWalkBegins = pedWalkBegins;
        //    UnknownTerminations = unknownTerminations;
        //}

        public int PhaseNumber { get; set; }
        public ICollection<ITimestamp> GapOuts { get; internal set; }
        public ICollection<ITimestamp> MaxOuts { get; internal set; }
        public ICollection<ITimestamp> ForceOffs { get; internal set; }
        public ICollection<ITimestamp> PedWalkBegins { get; internal set; }
        public ICollection<ITimestamp> UnknownTerminations { get; internal set; }

        public override string ToString()
        {
            return $"{PhaseNumber} - {GapOuts?.Count} - {MaxOuts?.Count} - {ForceOffs?.Count} - {PedWalkBegins?.Count} - {UnknownTerminations?.Count}";
        }
    }

    public class IdentifyTerminationTypesAndTimes : TransformProcessStepBase<Tuple<Approach, int, IEnumerable<ControllerEventLog>>, Tuple<Approach, int, Phase>>
    {
        public IdentifyTerminationTypesAndTimes(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<Tuple<Approach, int, Phase>> Process(Tuple<Approach, int, IEnumerable<ControllerEventLog>> input, CancellationToken cancelToken = default)
        {
            var filters = new List<int>()
            {
                (int)DataLoggerEnum.PhaseGapOut,
                (int)DataLoggerEnum.PhaseMaxOut,
                (int)DataLoggerEnum.PhaseForceOff,
                (int)DataLoggerEnum.PhaseGreenTermination
            };
            
            var approach = input.Item1;
            var phase = input.Item2;
            var logs = input.Item3
                .Where(w => w.EventParam == phase)
                .Where(w => filters.Contains(w.EventCode))
                .OrderBy(o => o.Timestamp).ToList();

            var consec = 3;

            var test1 = logs.Where((w, i) => (i - consec) >= 0 && logs.Skip(i - consec).Take(consec).All(a => a.EventCode == w.EventCode)).ToList();
            //var test2 = logs.Where((w, i) => (i - consec) >= 0).ToList();
            //var test3 = logs.Where((w, i) => logs.Skip(i).Take(2).All(a => a.EventCode == w.EventCode)).ToList();

            var stuff = new Phase()
            {
                PhaseNumber = phase,
                GapOuts = test1.Where(w => w.EventCode == (int)DataLoggerEnum.PhaseGapOut).Cast<ITimestamp>().ToList(),
                MaxOuts = test1.Where(w => w.EventCode == (int)DataLoggerEnum.PhaseMaxOut).Cast<ITimestamp>().ToList(),
                ForceOffs = test1.Where(w => w.EventCode == (int)DataLoggerEnum.PhaseForceOff).Cast<ITimestamp>().ToList(),
                UnknownTerminations = test1.Where(w => w.EventCode == (int)DataLoggerEnum.PhaseGreenTermination).Cast<ITimestamp>().ToList(),
            };

            var result = Tuple.Create(approach, phase, stuff);

            return Task.FromResult(result);
        }
    }
}
