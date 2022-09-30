using System;

namespace ATSPM.Application.Reports.ViewModels.PerdueCoordinationDiagram;

public class VolumePerHour
{
    public VolumePerHour(DateTime startTime, double volume)
    {
        StartTime = startTime;
        Volume = volume;
    }

    public DateTime StartTime { get; internal set; }
    public double Volume { get; internal set; }

}
