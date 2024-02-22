using ATSPM.Application.Business.Common;
using System.Runtime.Serialization;

namespace ATSPM.Application.Business.PedDelay
{
    [DataContract]
    public class PedDelayOptions : OptionsBase
    {
        public int TimeBuffer { get; set; }
        public bool ShowPedBeginWalk { get; set; }
        public bool ShowCycleLength { get; set; }
        public bool ShowPercentDelay { get; set; }
        public bool ShowPedRecall { get; set; }
        public int PedRecallThreshold { get; set; }
    }
}