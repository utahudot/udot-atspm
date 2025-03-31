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
        public DateTime MinGreen { get; set; }
        public DateTime YellowEvent { get; set; }
        public DateTime RedEvent { get; set; }
        public DateTime EndRedClearanceEvent { get; set; }
        public Double DurationSeconds { get { return (RedEvent - GreenEvent).TotalSeconds; } }
        public Double MinTime { get { return MinGreenDurationSeconds + YellowDurationSeconds + RedDurationSeconds; } }
        public Double GreenDurationSeconds { get { return (YellowEvent - GreenEvent).TotalSeconds; } }
        public Double MinGreenDurationSeconds { get { return (MinGreen - GreenEvent).TotalSeconds; } }
        public Double YellowDurationSeconds { get { return (RedEvent - YellowEvent).TotalSeconds; } }
        public Double RedDurationSeconds { get { return (EndRedClearanceEvent - RedEvent).TotalSeconds; } }
        public short? TerminationEvent { get; set; }
    }
}
