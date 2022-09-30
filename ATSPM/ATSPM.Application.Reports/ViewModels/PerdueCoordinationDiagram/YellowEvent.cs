using System;

namespace ATSPM.Application.Reports.ViewModels.PerdueCoordinationDiagram;

public class YellowEvent
{
    public YellowEvent(DateTime startTime, double seconds)
    {
        StartTime = startTime;
        Seconds = seconds;
    }

    public DateTime StartTime { get; internal set; }
    public double Seconds { get; internal set; }
}
