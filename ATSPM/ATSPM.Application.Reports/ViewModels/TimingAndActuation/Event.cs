using System;

namespace ATSPM.Application.Reports.ViewModels.TimingAndActuation;

public class Event
{
    public Event(DateTime startTime, double seconds)
    {
        StartTime = startTime;
        Seconds = seconds;
    }

    public DateTime StartTime { get; internal set; }
    public double Seconds { get; internal set; }
}