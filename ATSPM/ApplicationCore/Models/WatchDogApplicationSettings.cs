using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Models
{
    public class WatchDogApplicationSettings : ApplicationSettings
    {
        public int ConsecutiveCount { get; set; }
        public int MinPhaseTerminations { get; set; }
        public double PercentThreshold { get; set; }
        public int MaxDegreeOfParallelism { get; set; }
        public int ScanDayStartHour { get; set; }
        public int ScanDayEndHour { get; set; }
        public int PreviousDayPMPeakStart { get; set; }
        public int PreviousDayPMPeakEnd { get; set; }
        public int MinimumRecords { get; set; }
        public bool WeekdayOnly { get; set; }
        public string DefaultEmailAddress { get; set; }
        public string FromEmailAddress { get; set; }
        public int LowHitThreshold { get; set; }
        public string EmailServer { get; set; }
        public int MaximumPedestrianEvents { get; set; }
        public bool EmailAllErrors { get; set; }
        public WatchDogApplicationSettings()
        {
            EmailAllErrors = false;
        }

    }
}
