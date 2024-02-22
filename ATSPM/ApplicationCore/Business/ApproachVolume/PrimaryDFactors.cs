using Newtonsoft.Json;
using System;

namespace ATSPM.Application.Business.ApproachVolume;

public class DFactors
{
    public DFactors(DateTime startTime, double dFactor)
    {
        StartTime = startTime;
        DFactor = dFactor;
    }

    public DateTime StartTime { get; set; }
    public double DFactor { get; set; }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }

}
