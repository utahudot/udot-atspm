using System;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.ArrivalOnRed
{
    [DataContract]
    public class ArrivalOnRedOptions
    {
        public int SelectedBinSize { get; set; }
        public bool ShowPlanStatistics { get; set; }
        public string SignalIdentifier { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}