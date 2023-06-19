using Reports.Business.Common;
using System;

namespace ATSPM.Application.Reports.Business.SplitMonitor
{
    public class SplitMonitorOptions 
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public String SignalId { get; set; }
        public int PercentileSplit { get; set; }


    }
}