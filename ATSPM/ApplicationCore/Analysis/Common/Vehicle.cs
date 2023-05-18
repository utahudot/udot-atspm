using ATSPM.Application.Enums;
using System;

namespace ATSPM.Application.Analysis.Common
{
    public class Vehicle
    {
        public Vehicle() { }

        public Vehicle(CorrectedDetectorEvent detectorEvent, RedToRedCycle redToRedCycle)
        {
            SignalId = detectorEvent.Detector.Approach?.Signal?.SignalId;
            CorrectedTimeStamp = detectorEvent.CorrectedTimeStamp;
            DetChannel = detectorEvent.Detector.DetChannel;
            Phase = redToRedCycle.Phase;
            StartTime = redToRedCycle.StartTime;
            EndTime = redToRedCycle.EndTime;
            YellowEvent = redToRedCycle.YellowEvent;
            GreenEvent = redToRedCycle.GreenEvent;
        }

        public string SignalId { get; set; }

        //HACK: this could come from detectorEvent or Cycle! need to validate that they are the same!
        public int Phase { get; set; }
        public int DetChannel { get; set; }
        public DateTime CorrectedTimeStamp { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime GreenEvent { get; set; }
        public DateTime YellowEvent { get; set; }

        public double Delay => ArrivalType == ArrivalType.ArrivalOnRed ? (GreenEvent - CorrectedTimeStamp).TotalSeconds : 0;

        public ArrivalType ArrivalType
        {
            get
            {
                if (CorrectedTimeStamp < GreenEvent && CorrectedTimeStamp >= StartTime)
                {
                    return ArrivalType.ArrivalOnRed;
                }

                else if (CorrectedTimeStamp >= GreenEvent && CorrectedTimeStamp < YellowEvent)
                {
                    return ArrivalType.ArrivalOnGreen;
                }

                else if (CorrectedTimeStamp >= YellowEvent && CorrectedTimeStamp <= EndTime)
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
            return $"{SignalId}-{DetChannel}-{CorrectedTimeStamp:yyyy-MM-dd'T'HH:mm:ss.f}-{ArrivalType}-{Delay}";
        }
    }
}
