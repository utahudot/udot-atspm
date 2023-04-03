using System;

namespace ATSPM.Application.Reports.Business.SplitFail
{
    public class SplitFailDetectorActivation

    {
        public DateTime DetectorOn { get; set; }
        public DateTime DetectorOff { get; set; }
        public bool ReviewedForOverlap { get; set; } = false;

        public double DurationInMilliseconds
        {
            get
            {
                return (DetectorOff - DetectorOn).TotalMilliseconds;
            }
        }
    }
}