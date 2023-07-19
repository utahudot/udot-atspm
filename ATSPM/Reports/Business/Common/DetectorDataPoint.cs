using System;

namespace ATSPM.Application.Reports.Business.Common
{
    public enum ArrivalType
    {
        ArrivalOnGreen,
        ArrivalOnYellow,
        ArrivalOnRed
    }

    public class DetectorDataPoint
    {
        public DetectorDataPoint(DateTime startDate, DateTime eventTime, DateTime greenEvent, DateTime yellowEvent)
        {
            TimeStamp = eventTime;
            StartOfCycle = startDate;
            YPointSeconds = (TimeStamp - StartOfCycle).TotalSeconds;
            YellowEvent = yellowEvent;
            GreenEvent = greenEvent;
            SetDataPointProperties();
        }

        private void SetDataPointProperties()
        {
            //if the detector hit is before greenEvent
            if (TimeStamp < GreenEvent)
            {
                var test = (GreenEvent - TimeStamp);
                DelaySeconds = (GreenEvent - TimeStamp).TotalSeconds;
                ArrivalType = ArrivalType.ArrivalOnRed;
            }
            //if the detector hit is After green, but before yellow
            else if (TimeStamp >= GreenEvent && TimeStamp < YellowEvent)
            {
                DelaySeconds = 0;
                ArrivalType = ArrivalType.ArrivalOnGreen;
            }
            //if the event time is after yellow
            else if (TimeStamp >= YellowEvent)
            {
                DelaySeconds = 0;
                ArrivalType = ArrivalType.ArrivalOnYellow;
            }
        }

        //Represents a time span from the start of the red to red cycle
        public double YPointSeconds { get; private set; }

        public DateTime StartOfCycle { get; }

        //The actual time of the detector activation
        public DateTime TimeStamp { get; private set; }

        public DateTime YellowEvent { get; }

        public DateTime GreenEvent { get; }

        public double DelaySeconds { get; set; }

        public ArrivalType ArrivalType { get; set; }

        public void AddSeconds(int seconds)
        {
            TimeStamp = TimeStamp.AddSeconds(seconds);
            YPointSeconds = (TimeStamp - StartOfCycle).TotalSeconds;
            SetDataPointProperties();
        }
    }
}