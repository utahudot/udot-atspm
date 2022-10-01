using System;

namespace ATSPM.Application.Reports.ViewModels.SplitMonitor;

public class MaxOut
{
    public MaxOut(DateTime startTime, double seconds)
    {
        StartTime = startTime;
        Seconds = seconds;
    }

    public DateTime StartTime { get; internal set; }
    public double Seconds { get; internal set; }
}
