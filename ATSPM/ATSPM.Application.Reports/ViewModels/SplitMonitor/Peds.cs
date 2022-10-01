namespace ATSPM.Application.Reports.ViewModels.SplitMonitor;

public class Peds
{
    public Peds(string startTime, double seconds)
    {
        StartTime = startTime;
        Seconds = seconds;
    }

    public string StartTime { get; internal set; }
    public double Seconds { get; internal set; }
}
