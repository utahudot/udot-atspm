﻿namespace ATSPM.Application.Reports.ViewModels.SplitMonitor;

public class SplitMonitorUnknown
{
    public SplitMonitorUnknown(string startTime, double seconds)
    {
        StartTime = startTime;
        Seconds = seconds;
    }

    public string StartTime { get; internal set; }
    public double Seconds { get; internal set; }
}