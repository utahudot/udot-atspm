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
    public class Phase : ISignalPhaseLayer
    {
        public Phase() { }

        public Phase(IEnumerable<ControllerEventLog> terminationEvents)
        {
            TerminationEvents.AddRange(terminationEvents);
        }

        public string SignalIdentifier { get; set; }
        
        public int PhaseNumber { get; set; }

        public List<ControllerEventLog> TerminationEvents { get; set; } = new List<ControllerEventLog>();

        public IReadOnlyList<ITimestamp> GapOuts => TerminationEvents.Where(w => w.EventCode == (int)DataLoggerEnum.PhaseGapOut).Cast<ITimestamp>().ToList();
        public IReadOnlyList<ITimestamp> MaxOuts => TerminationEvents.Where(w => w.EventCode == (int)DataLoggerEnum.PhaseMaxOut).Cast<ITimestamp>().ToList();
        public IReadOnlyList<ITimestamp> ForceOffs => TerminationEvents.Where(w => w.EventCode == (int)DataLoggerEnum.PhaseForceOff).Cast<ITimestamp>().ToList();
        public IReadOnlyList<ITimestamp> PedWalkBegins => TerminationEvents.Where(w => w.EventCode == (int)DataLoggerEnum.PedestrianBeginWalk).Cast<ITimestamp>().ToList();
        public IReadOnlyList<ITimestamp> UnknownTerminations => TerminationEvents.Where(w => w.EventCode == (int)DataLoggerEnum.PhaseGreenTermination).Cast<ITimestamp>().ToList();

        public override string ToString()
        {
            return $"{PhaseNumber} - {TerminationEvents.Count} - {GapOuts?.Count} - {MaxOuts?.Count} - {ForceOffs?.Count} - {PedWalkBegins?.Count} - {UnknownTerminations?.Count}";
        }
    }

    public class IdentifyTerminationTypesAndTimes : TransformProcessStepBase<Tuple<Approach, int, IEnumerable<ControllerEventLog>>, Tuple<Approach, int, Phase>>
    {
        public IdentifyTerminationTypesAndTimes(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<Tuple<Approach, int, Phase>> Process(Tuple<Approach, int, IEnumerable<ControllerEventLog>> input, CancellationToken cancelToken = default)
        {
            var consec = 3;

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

            //if there are two consecutive )DataLoggerEnum.PhaseGreenTermination then the second denotes an unknown termination
            var consecGreenTerminations = logs.GetLastConsecutiveEvent(2).Where(w => w.EventCode == (int)DataLoggerEnum.PhaseGreenTermination).ToList();

            //remove DataLoggerEnum.PhaseGreenTermination and get the consecutive terminations
            var consecTerminations = logs.Where(r => r.EventCode != (int)DataLoggerEnum.PhaseGreenTermination).GetLastConsecutiveEvent(consec).ToList();

            var stuff = new Phase(consecTerminations.Union(consecGreenTerminations))
            {
                SignalIdentifier = approach.Signal.SignalIdentifier,
                PhaseNumber = phase,
            };

            var result = Tuple.Create(approach, phase, stuff);

            return Task.FromResult(result);
        }
    }
}
