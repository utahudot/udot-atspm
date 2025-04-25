using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utah.Udot.Atspm.Business.TransitSignalPriority
{
    public class TransitSignalPriorityPlan
    {
        public int PlanNumber { get; set; }
        public List<TransitSignalPhase> Phases { get; set; } = new List<TransitSignalPhase>();
        public int NumberOfCycles { get; set; }
    }
}
