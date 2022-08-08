using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class SignalPlanAggregation
    {
        public string SignalId { get; set; } = null!;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int PlanNumber { get; set; }
    }
}
