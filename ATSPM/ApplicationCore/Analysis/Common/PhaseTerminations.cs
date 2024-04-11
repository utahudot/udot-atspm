using ATSPM.Data.Enums;
using ATSPM.Data.Interfaces;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Analysis.Common
{
    public class PhaseTerminations : ILocationPhaseLayer
    {
        public PhaseTerminations() { }

        public PhaseTerminations(IEnumerable<ControllerEventLog> terminationEvents)
        {
            TerminationEvents.AddRange(terminationEvents);
        }

        public string LocationIdentifier { get; set; }

        public int PhaseNumber { get; set; }

        public List<ControllerEventLog> TerminationEvents { get; set; } = new List<ControllerEventLog>();

        public IReadOnlyList<ITimestamp> GapOuts => TerminationEvents.Where(w => w.EventCode == (int)IndianaEnumerations.PhaseGapOut).Cast<ITimestamp>().ToList();
        public IReadOnlyList<ITimestamp> MaxOuts => TerminationEvents.Where(w => w.EventCode == (int)IndianaEnumerations.PhaseMaxOut).Cast<ITimestamp>().ToList();
        public IReadOnlyList<ITimestamp> ForceOffs => TerminationEvents.Where(w => w.EventCode == (int)IndianaEnumerations.PhaseForceOff).Cast<ITimestamp>().ToList();
        public IReadOnlyList<ITimestamp> PedWalkBegins => TerminationEvents.Where(w => w.EventCode == (int)IndianaEnumerations.PedestrianBeginWalk).Cast<ITimestamp>().ToList();
        public IReadOnlyList<ITimestamp> UnknownTerminations => TerminationEvents.Where(w => w.EventCode == (int)IndianaEnumerations.PhaseGreenTermination).Cast<ITimestamp>().ToList();

        public override string ToString()
        {
            return $"{PhaseNumber} - {TerminationEvents.Count} - {GapOuts?.Count} - {MaxOuts?.Count} - {ForceOffs?.Count} - {PedWalkBegins?.Count} - {UnknownTerminations?.Count}";
        }
    }
}
