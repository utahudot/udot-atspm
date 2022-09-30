using System;

namespace ATSPM.Application.Reports.ViewModels.ApproachVolume;

public class PrimaryDFactors
{
    public PrimaryDFactors(DateTime startTime, double dFactor)
    {
        StartTime = startTime;
        DFactor = dFactor;
    }

    public DateTime StartTime { get; internal set; }
    public double DFactor { get; internal set; }

}
