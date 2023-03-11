using System;

namespace ATSPM.Application.Reports.Business.SplitMonitor;

public class SplitMonitorUnknown
{
    public SplitMonitorUnknown(DateTime startTime, double seconds)
    {
        StartTime = startTime;
        Seconds = seconds;
    }

    public DateTime StartTime { get; internal set; }
    public double Seconds { get; internal set; }
}
