using ATSPM.Domain.Common;
using System;

namespace ATSPM.Application.Analysis.Common
{
    public class RedToRedCycle : StartEndRange
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

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        //public double GreenLineY { get; }
        //public double YellowLineY { get; }
        //public double RedLineY { get; }
        public DateTime GreenEvent { get; set; }
        public DateTime YellowEvent { get; set; }

        public string SignalId { get; set; }
        public int Phase { get; set; }

        //public ICollection<ControllerEventLog> VehicleEvents { get; set; } = new List<ControllerEventLog>();


        public double TotalGreenTime => (YellowEvent - GreenEvent).TotalSeconds;
        public double TotalYellowTime => (End - YellowEvent).TotalSeconds;
        public double TotalRedTime => (GreenEvent - Start).TotalSeconds;
        public double TotalTime => (End - Start).TotalSeconds;
        //public double TotalGreenTimeMilliseconds => (YellowEvent - GreenEvent).TotalMilliseconds;
        //public double TotalYellowTimeMilliseconds => (End - YellowEvent).TotalMilliseconds;
        //public double TotalRedTimeMilliseconds => (GreenEvent - Start).TotalMilliseconds;
        //public double TotalTimeMilliseconds => (End - Start).TotalMilliseconds;

        public override bool InRange(DateTime time)
        {
            return time >= Start && time <= End;
        }

        public override string ToString()
        {
            return $"Signal: {SignalId} Phase: {Phase} Start: {Start:yyyy-MM-dd'T'HH:mm:ss.f} Green: {GreenEvent:yyyy-MM-dd'T'HH:mm:ss.f} Yellow: {YellowEvent:yyyy-MM-dd'T'HH:mm:ss.f} End: {End:yyyy-MM-dd'T'HH:mm:ss.f} " +
                $"- {TotalRedTime} - {TotalYellowTime} - {TotalGreenTime}";
        }
    }
}
