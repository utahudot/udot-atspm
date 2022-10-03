using System;

namespace ATSPM.Application.Reports.ViewModels.PerdueCoordinationDiagram;

public class PerdueCoordinationPlan : Plan
{
    public PerdueCoordinationPlan(
        string planNumber,
        DateTime startTime,
        DateTime endTime,
        double percentArrivalOnGreen,
        double percentGreenTime,
        double platoonatio) : base(planNumber, startTime, endTime)
    {
        PercentArrivalOnGreen = percentArrivalOnGreen;
        PercentGreenTime = percentGreenTime;
        Platoonatio = platoonatio;
    }

    public double PercentArrivalOnGreen { get; internal set; }
    public double PercentGreenTime { get; internal set; }
    public double Platoonatio { get; internal set; }
}
