using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Business.Common;

namespace Utah.Udot.Atspm.Business.TransitSignalPriority
{
    public class TransitSignalPriorityBasicPlan:Plan
    {
        public TransitSignalPriorityBasicPlan(string planNumber, DateTime startTime, DateTime endTime) : base(planNumber, startTime, endTime)
        {
        }

        public SortedDictionary<int,int> ProgrammedSplits { get; set; }
    }
}
