using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Utah.Udot.Atspm.Business.Common.CycleSplitFail;

namespace Utah.Udot.Atspm.Business.TransitSignalPriority
{
    public class TransitSignalPriorityCycle
    {
        public int PhaseNumber { get; set; }
        public DateTime GreenEvent { get; set; }
        public DateTime YellowEvent { get; set; }
        public DateTime RedEvent { get; set; }
        public DateTime EndRedClearanceEvent { get; set; }
        public Double DurationSeconds { get { return (RedEvent - GreenEvent).Seconds; } }
        public Double GreenDurationSeconds { get { return (YellowEvent - GreenEvent).Seconds; } }
        public Double YellowDurationSeconds { get { return (RedEvent - YellowEvent).Seconds; } }
        public Double RedDurationSeconds { get { return (EndRedClearanceEvent - RedEvent).Seconds; } }
        public short? TerminationEvent { get; set; }
    }
}
