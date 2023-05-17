using System;

namespace ATSPM.Application.Analysis.Common
{
    public class RedToRedCycle
    {
        //public RedToRedCycle(DateTime firstRedEvent, DateTime greenEvent, DateTime yellowEvent, DateTime lastRedEvent)
        //{
        //    StartTime = firstRedEvent;
        //    GreenEvent = greenEvent;
        //    GreenLineY = (greenEvent - StartTime).TotalSeconds;
        //    YellowEvent = yellowEvent;
        //    YellowLineY = (yellowEvent - StartTime).TotalSeconds;
        //    EndTime = lastRedEvent;
        //    RedLineY = (lastRedEvent - StartTime).TotalSeconds;
        //    //PreemptCollection = new List<DetectorDataPoint>();
        //}

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        //public double GreenLineY { get; }
        //public double YellowLineY { get; }
        //public double RedLineY { get; }
        public DateTime GreenEvent { get; set; }
        public DateTime YellowEvent { get; set; }

        public string SignalId { get; set; }
        public int Phase { get; set; }

        //public ICollection<ControllerEventLog> VehicleEvents { get; set; } = new List<ControllerEventLog>();


        public double TotalGreenTime => (YellowEvent - GreenEvent).TotalSeconds;
        public double TotalYellowTime => (EndTime - YellowEvent).TotalSeconds;
        public double TotalRedTime => (GreenEvent - StartTime).TotalSeconds;
        public double TotalTime => (EndTime - StartTime).TotalSeconds;
        public double TotalGreenTimeMilliseconds => (YellowEvent - GreenEvent).TotalMilliseconds;
        public double TotalYellowTimeMilliseconds => (EndTime - YellowEvent).TotalMilliseconds;
        public double TotalRedTimeMilliseconds => (GreenEvent - StartTime).TotalMilliseconds;
        public double TotalTimeMilliseconds => (EndTime - StartTime).TotalMilliseconds;

        public override string ToString()
        {
            return $"Signal: {SignalId} Phase: {Phase} Start: {StartTime:yyyy-MM-dd'T'HH:mm:ss.f} Green: {GreenEvent:yyyy-MM-dd'T'HH:mm:ss.f} Yellow: {YellowEvent:yyyy-MM-dd'T'HH:mm:ss.f} End: {EndTime:yyyy-MM-dd'T'HH:mm:ss.f}";
        }
    }
}
