using System;

namespace ATSPM.Application.Analysis.Common
{
    public class CorrectedDetectorEvent
    {
        public string SignalId { get; set; }
        public DateTime TimeStamp { get; set; }
        public int DetChannel { get; set; }

        //public Detector Detector { get; set; }

        public override string ToString()
        {
            return $"{SignalId}-{DetChannel}-{TimeStamp:yyyy-MM-dd'T'HH:mm:ss.f}";
        }
    }
}
