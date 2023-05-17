using System;

namespace ATSPM.Application.Analysis.PreemptionDetails
{
    public abstract class PreempDetailValueBase
    {
        public string SignalId { get; set; }
        public int PreemptNumber { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeSpan Seconds { get; set; }

        public override string ToString() => $"{GetType().Name}-{SignalId}-{PreemptNumber}-{Start}-{End}-{Seconds}";
    }
}
