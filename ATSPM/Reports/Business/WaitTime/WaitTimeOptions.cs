using System;

namespace ATSPM.Application.Reports.Business.WaitTime
{
    public class WaitTimeOptions
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string SignalIdentifier { get; set; }
        public int BinSize { get; set; }


    }
}