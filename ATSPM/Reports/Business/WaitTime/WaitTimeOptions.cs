using System;

namespace ATSPM.Application.Reports.Business.WaitTime
{
    public class WaitTimeOptions
    {
        public const int PHASE_BEGIN_GREEN = 1;
        public const int PHASE_END_RED_CLEARANCE = 11;
        public const int PHASE_CALL_REGISTERED = 43;
        public const int PHASE_CALL_DROPPED = 44;

        public WaitTimeOptions()
        {
        }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string SignalIdentifier { get; set; }
        public int BinSize { get; set; }


    }
}