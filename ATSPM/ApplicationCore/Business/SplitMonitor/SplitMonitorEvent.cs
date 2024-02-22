using System;

namespace ATSPM.Application.Business.SplitMonitor;

public class SplitMonitorEvent
{
    public SplitMonitorEvent(DateTime startTime, double seconds)
    {
        StartTime = startTime;
        Seconds = seconds;
    }

    public DateTime StartTime { get; internal set; }
    public double Seconds { get; internal set; }
}
