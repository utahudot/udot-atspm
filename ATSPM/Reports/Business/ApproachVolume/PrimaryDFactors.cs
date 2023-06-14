using Newtonsoft.Json;
using System;

namespace ATSPM.Application.Reports.Business.ApproachVolume;

public class DFactors
{
    public DFactors(DateTime startTime, double dFactor)
    {
        StartTime = startTime;
        DFactor = dFactor;
    }

    public DateTime StartTime { get; set; }
    [JsonProperty("dFactor")]
    public double DFactor { get; set; }

}
