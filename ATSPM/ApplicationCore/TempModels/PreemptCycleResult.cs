using System;

namespace ATSPM.Application.TempModels
{
    public class PreemptCycleResult
    {
        public DateTime InputOff { get; set; }

        public DateTime InputOn { get; set; }

        public DateTime? GateDown { get; set; }

        public double CallMaxOut { get; set; }

        public double Delay { get; set; }

        public double TimeToService { get; set; }

        public double DwellTime { get; set; }

        public double TrackClear { get; set; }
    }
}
