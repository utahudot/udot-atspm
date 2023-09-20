using System;

namespace ATSPM.Application.Reports.Business.SplitFail
{
    public class SplitFailOptions
    {
        public int FirstSecondsOfRed { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int MetricTypeId { get; set; } = 12;
        public string SignalIdentifier { get; set; }
        public bool UsePermissivePhase { get; set; }


    }
}