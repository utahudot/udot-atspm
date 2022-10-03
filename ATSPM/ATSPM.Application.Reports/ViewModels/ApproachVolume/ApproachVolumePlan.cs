using System;

namespace ATSPM.Application.Reports.ViewModels.ApproachVolume;

public class ApproachVolumePlan : Plan
{
    public ApproachVolumePlan(
        int averageSpeed,
        int standardDeviation,
        int eightyFifthPercentile,
        int fifteenthPercentile,
        DateTime start,
        DateTime end,
        string planNumber) : base(planNumber, start, end)
    {
        AverageSpeed = averageSpeed;
        StandardDeviation = standardDeviation;
        EightyFifthPercentile = eightyFifthPercentile;
        FifteenthPercentile = fifteenthPercentile;
    }

    public int AverageSpeed { get; internal set; }
    public int StandardDeviation { get; internal set; }
    public int EightyFifthPercentile { get; internal set; }
    public int FifteenthPercentile { get; internal set; }

}
