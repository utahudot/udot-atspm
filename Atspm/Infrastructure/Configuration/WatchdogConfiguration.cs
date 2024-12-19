using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    public class WatchdogConfiguration
    {
        public DateTime ScanDate { get; set; } = DateTime.Today.AddDays(-1);
        public int PreviousDayPMPeakStart { get; set; } = 18;
        public int PreviousDayPMPeakEnd { get; set; } = 17;
        public bool WeekdayOnly { get; set; } = true;
        public int ScanDayStartHour { get; set; }
        public int ScanDayEndHour { get; set; }
        public int ConsecutiveCount { get; set; } = 3;
        public int LowHitThreshold { get; set; } = 50;
        public int MaximumPedestrianEvents { get; set; } = 200;
        public int MinimumRecords { get; set; } = 500;
        public int MinPhaseTerminations { get; set; } = 50;
        public double PercentThreshold { get; set; } = .9;
        public int RampMainlineStartHour { get; set; } = 15;
        public int RampMainlineEndHour { get; set; } = 19;
        public int RampStuckQueueStartHour { get; set; } = 1;
        public int RampStuckQueueEndHour { get; set; } = 4;

        public bool EmailAllErrors { get; set; }
        public string DefaultEmailAddress { get; set; }

        public DateTime AnalysisStart => ScanDate.Date + new TimeSpan(ScanDayStartHour, 0, 0);
        public DateTime AnalysisEnd => ScanDate.Date + new TimeSpan(ScanDayEndHour, 0, 0);
    }

    //TODO: this needs to be added into WatchdogConfiguration and config json needs to be changed from flat
    public class WatchdogLoggingOptions
    {
        public DateTime ScanDate { get; set; }
        public int ScanDayStartHour { get; set; }
        public int ScanDayEndHour { get; set; }
        public int ConsecutiveCount { get; set; }
        public int MinPhaseTerminations { get; set; }
        public double PercentThreshold { get; set; }
        public int PreviousDayPMPeakStart { get; set; }
        public int PreviousDayPMPeakEnd { get; set; }
        public int MinimumRecords { get; set; }
        public int LowHitThreshold { get; set; }
        public int MaximumPedestrianEvents { get; set; }
        public bool WeekdayOnly { get; set; }
        public int RampMainlineStartHour { get; set; }
        public int RampMainlineEndHour { get; set; }
        public int RampStuckQueueStartHour { get; set; }
        public int RampStuckQueueEndHour { get; set; }


        public DateTime AnalysisStart => ScanDate.Date + new TimeSpan(ScanDayStartHour, 0, 0);

        public DateTime AnalysisEnd => ScanDate.Date + new TimeSpan(ScanDayEndHour, 0, 0);
    }

    //TODO: this needs to be added into WatchdogConfiguration and config json needs to be changed from flat
    public class WatchdogEmailOptions
    {
        public DateTime ScanDate { get; set; }
        public int ScanDayStartHour { get; set; }
        public int ScanDayEndHour { get; set; }
        public int PreviousDayPMPeakStart { get; set; }
        public int PreviousDayPMPeakEnd { get; set; }
        public int RampMainlineStartHour { get; set; }
        public int RampMainlineEndHour { get; set; }
        public int RampStuckQueueStartHour { get; set; }
        public int RampStuckQueueEndHour { get; set; }
        public bool EmailAllErrors { get; set; }
        public string DefaultEmailAddress { get; set; }
        public bool WeekdayOnly { get; set; }
    }
}
