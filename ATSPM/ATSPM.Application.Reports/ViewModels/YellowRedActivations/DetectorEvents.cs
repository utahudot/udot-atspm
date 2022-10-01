namespace ATSPM.Application.Reports.ViewModels.YellowRedActivations
{
    public class DetectorEvents
    {
        public DetectorEvents(string startTime, int seconds)
        {
            StartTime = startTime;
            Seconds = seconds;
        }

        public string StartTime { get; internal set; }
        public int Seconds { get; internal set; }
    }
}