namespace ATSPM.Application.Reports.Business.YellowRedActivations
{
    public class YellowEvents
    {
        public YellowEvents(string startTime, double seconds)
        {
            StartTime = startTime;
            Seconds = seconds;
        }

        public string StartTime { get; internal set; }
        public double Seconds { get; internal set; }
    }
}