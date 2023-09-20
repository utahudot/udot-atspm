using System;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.PedDelay
{
    [DataContract]
    public class PedDelayOptions
    {

        public string SignalIdentifier { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int TimeBuffer { get; set; }
        public bool ShowPedBeginWalk { get; set; }
        public bool ShowCycleLength { get; set; }
        public bool ShowPercentDelay { get; set; }
        public bool ShowPedRecall { get; set; }
        public int PedRecallThreshold { get; set; }
    }
}