
using System;

namespace ATSPM.Application.Reports.ViewModels.GapVsDemand;

public class Failures
{
    public Failures(DateTime startTime, double percent)
    {
        StartTime = startTime;
        Percent = percent;
    }

    public DateTime StartTime { get; set; }
    public double Percent { get; set; }

}