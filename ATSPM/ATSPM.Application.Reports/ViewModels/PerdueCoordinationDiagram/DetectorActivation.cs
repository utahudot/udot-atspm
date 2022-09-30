using System;

namespace ATSPM.Application.Reports.ViewModels.PerdueCoordinationDiagram;

public class DetectorActivation
{
    public DetectorActivation(DateTime timestamp, double seconds)
    {
        Timestamp = timestamp;
        Seconds = seconds;
    }

    public DateTime Timestamp { get; internal set; }
    public double Seconds { get; internal set; }
}