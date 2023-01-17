using System;

namespace ATSPM.Application.Reports.ViewModels.ApproachVolume;

public class DFactors
{
    public DFactors(DateTime startTime, double dFactor)
    {
        StartTime = startTime;
        DFactor = dFactor;
    }

    public DateTime StartTime { get; internal set; }
    public double DFactor { get; internal set; }

}
