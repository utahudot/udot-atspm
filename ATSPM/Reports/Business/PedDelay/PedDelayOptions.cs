using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.PedDelay
{
    [DataContract]
    public class PedDelayOptions
    {
        //public PedDelayOptions(int approachId, DateTime startDate, DateTime endDate, int timeBuffer, bool showPedBeginWalk, bool showCycleLength, bool showPercentDelay, bool showPedRecall, int pedRecallThreshold)
        //{
        //    Start = startDate;
        //    End = endDate;
        //    ApproachId = approachId;
        //    Start = startDate;
        //    TimeBuffer = timeBuffer;
        //    ShowPedBeginWalk = showPedBeginWalk;
        //    ShowCycleLength = showCycleLength;
        //    ShowPercentDelay = showPercentDelay;
        //    ShowPedRecall = showPedRecall;
        //    PedRecallThreshold = pedRecallThreshold;
        //}

        public int ApproachId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int TimeBuffer { get; set; }
        public bool ShowPedBeginWalk { get; set; }
        public bool ShowCycleLength { get; set; }
        public bool ShowPercentDelay { get; set; }
        public bool ShowPedRecall { get; set; }
        public int PedRecallThreshold { get; set; }
        public bool UsePermissivePhase { get; set; }
    }
}