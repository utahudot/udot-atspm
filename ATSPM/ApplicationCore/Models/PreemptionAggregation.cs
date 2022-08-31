using System;
using System.Collections.Generic;

#nullable disable

namespace ATSPM.Application.Models
{
    public partial class PreemptionAggregation : ATSPMModelBase
    {
        public DateTime BinStartTime { get; set; }
        public string SignalId { get; set; }
        public int PreemptNumber { get; set; }
        public int PreemptRequests { get; set; }
        public int PreemptServices { get; set; }
    }
}
