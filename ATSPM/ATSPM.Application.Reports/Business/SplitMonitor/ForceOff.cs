using System;

namespace ATSPM.Application.Reports.Business.SplitMonitor;

public class SplitMonitorForceOff
{
    public SplitMonitorForceOff(DateTime startTime, double seconds)
    {
        StartTime = startTime;
        Seconds = seconds;
    }

    public DateTime StartTime { get; internal set; }
    public double Seconds { get; internal set; }
}
