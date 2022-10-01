using System;

namespace ATSPM.Application.Reports.ViewModels.SplitMonitor;

public class ForceOff
{
    public ForceOff(DateTime startTime, double seconds)
    {
        StartTime = startTime;
        Seconds = seconds;
    }

    public DateTime StartTime { get; internal set; }
    public double Seconds { get; internal set; }
}
