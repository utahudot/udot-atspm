using System;

namespace ATSPM.Application.Reports.Business.PhaseTermination
{
    public class PurduePhaseTerminationOptions
    {
        public int SelectedConsecutiveCount { get; set; }
        public string SignalIdentifier { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}