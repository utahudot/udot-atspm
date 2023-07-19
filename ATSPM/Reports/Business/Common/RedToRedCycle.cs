using System;

namespace ATSPM.Application.Reports.Business.Common
{
    /// <summary>
    ///     Data that represents a red to red cycle for a signal phase
    /// </summary>
    public class RedToRedCycle
    {
        public enum EventType
        {
            ChangeToRed,
            ChangeToGreen,
            ChangeToYellow,
            GreenTermination,
            BeginYellowClearance,
            EndYellowClearance,
            Unknown,
            ChangeToEndMinGreen,
            ChangeToEndOfRedClearance,
            OverLapDark
        }

        public RedToRedCycle(DateTime firstRedEvent, DateTime greenEvent, DateTime yellowEvent, DateTime lastRedEvent)
        {
            StartTime = firstRedEvent;
            GreenEvent = greenEvent;
            GreenLineY = (greenEvent - StartTime).TotalSeconds;
            YellowEvent = yellowEvent;
            YellowLineY = (yellowEvent - StartTime).TotalSeconds;
            EndTime = lastRedEvent;
            RedLineY = (lastRedEvent - StartTime).TotalSeconds;
        }

        public DateTime StartTime { get; }
        public DateTime EndTime { get; }
        public double GreenLineY { get; }
        public double YellowLineY { get; }

        public double RedLineY { get; }
        public DateTime GreenEvent { get; }

        public DateTime YellowEvent { get; }

        public double TotalGreenTimeSeconds => (YellowEvent - GreenEvent).TotalSeconds;
        public double TotalYellowTimeSeconds => (EndTime - YellowEvent).TotalSeconds;
        public double TotalRedTimeSeconds => (GreenEvent - StartTime).TotalSeconds;
        public double TotalTimeSeconds => (EndTime - StartTime).TotalSeconds;
        public double TotalGreenTimeMilliseconds => (YellowEvent - GreenEvent).TotalMilliseconds;
        public double TotalYellowTimeMilliseconds => (EndTime - YellowEvent).TotalMilliseconds;
        public double TotalRedTimeMilliseconds => (GreenEvent - StartTime).TotalMilliseconds;
        public double TotalTimeMilliseconds => (EndTime - StartTime).TotalMilliseconds;
    }
}