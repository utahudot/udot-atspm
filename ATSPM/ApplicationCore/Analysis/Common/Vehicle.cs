using ATSPM.Application.Enums;
using System;

namespace ATSPM.Application.Analysis.Common
{
    public class Vehicle : CorrectedDetectorEvent
    {
        public Vehicle() { }

        public Vehicle(CorrectedDetectorEvent detectorEvent, RedToRedCycle redToRedCycle)
        {
            if (detectorEvent.SignalId == redToRedCycle.SignalId)
            {
                SignalId = detectorEvent.SignalId;
                TimeStamp = detectorEvent.TimeStamp;
                DetChannel = detectorEvent.DetChannel;
                Phase = redToRedCycle.Phase;
                StartTime = redToRedCycle.StartTime;
                EndTime = redToRedCycle.EndTime;
                YellowEvent = redToRedCycle.YellowEvent;
                GreenEvent = redToRedCycle.GreenEvent;
            }
        }

        public int Phase { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime GreenEvent { get; set; }
        public DateTime YellowEvent { get; set; }

        public double Delay => ArrivalType == ArrivalType.ArrivalOnRed ? (GreenEvent - TimeStamp).TotalSeconds : 0;

        public ArrivalType ArrivalType
        {
            get
            {
                if (TimeStamp < GreenEvent && TimeStamp >= StartTime)
                {
                    return ArrivalType.ArrivalOnRed;
                }

                else if (TimeStamp >= GreenEvent && TimeStamp < YellowEvent)
                {
                    return ArrivalType.ArrivalOnGreen;
                }

                else if (TimeStamp >= YellowEvent && TimeStamp <= EndTime)
                {
                    return ArrivalType.ArrivalOnYellow;
                }

                return ArrivalType.Unknown;
            }
        }

        //public Detector Detector { get; set; }

        //public RedToRedCycle RedToRedCycle { get; set; }

        public override string ToString()
        {
            return $"{SignalId}-{DetChannel}-{TimeStamp:yyyy-MM-dd'T'HH:mm:ss.f}-{ArrivalType}-{Delay}";
        }
    }
}
