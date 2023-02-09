using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.PedDelay
{
    [DataContract]
    public class PedDelayOptions
    {
        public PedDelayOptions(int approachId, DateTime startDate, DateTime endDate, int timeBuffer, bool showPedBeginWalk, bool showCycleLength, bool showPercentDelay, bool showPedRecall, int pedRecallThreshold)
        {
            StartDate = startDate;
            EndDate = endDate;
            ApproachId = approachId;
            StartDate = startDate;
            TimeBuffer = timeBuffer;
            ShowPedBeginWalk = showPedBeginWalk;
            ShowCycleLength = showCycleLength;
            ShowPercentDelay = showPercentDelay;
            ShowPedRecall = showPedRecall;
            PedRecallThreshold = pedRecallThreshold;
        }

        public int ApproachId { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        [DataMember]
        [Display(Name = "Time Buffer Between Unique Ped Detections")]
        public int TimeBuffer { get; set; }

        [DataMember]
        [Display(Name = "Show Ped Begin Walk")]
        public bool ShowPedBeginWalk { get; set; }

        [DataMember]
        [Display(Name = "Show Cycle Length")]
        public bool ShowCycleLength { get; set; }

        [DataMember]
        [Display(Name = "Show Percent Delay")]
        public bool ShowPercentDelay { get; set; }

        [DataMember]
        [Display(Name = "Show Ped Recall")]
        public bool ShowPedRecall { get; set; }

        [DataMember]
        [Display(Name = "Ped Recall Threshold (Percent)")]
        public int PedRecallThreshold { get; set; }


    }
}