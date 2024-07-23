using ATSPM.Application.Business.Common;
using ATSPM.Data.Enums;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.ApproachVolume;

/// <summary>
/// Approach Volume Chart
/// </summary>
public class ApproachVolumeResult : LocationResult
{
    public ApproachVolumeResult(
        string locationId,
        DateTime start,
        DateTime end,
        DirectionTypes directionType) : base(locationId, start, end)
    {
        PrimaryDirectionName = directionType.ToString();
    }

    public ApproachVolumeResult(
        string locationIdentifier,
        DateTime start,
        DateTime end,
        string detectorType,
        int distanceFromStopBar,
        string primaryDirectionName,
        string opposingDirectionName) : base(locationIdentifier, start, end)
    {
        DetectorType = detectorType;
        DistanceFromStopBar = distanceFromStopBar;
        PrimaryDirectionName = primaryDirectionName;
        OpposingDirectionName = opposingDirectionName;
    }

    public string PrimaryDirectionName { get; set; }
    public string OpposingDirectionName { get; set; }
    public int DistanceFromStopBar { get; set; }
    public string DetectorType { get; set; }
    public ICollection<DataPointForInt> PrimaryDirectionVolumes { get; set; }
    public ICollection<DataPointForInt> OpposingDirectionVolumes { get; set; }
    public ICollection<DataPointForInt> CombinedDirectionVolumes { get; set; }
    public ICollection<DataPointForDouble> PrimaryDFactors { get; set; }
    public ICollection<DataPointForDouble> OpposingDFactors { get; set; }
    public SummaryData SummaryData { get; set; }

}

public class SummaryData
{

    public string PeakHour { get; set; }
    public double KFactor { get; set; }
    public int PeakHourVolume { get; set; }
    public double PeakHourFactor { get; set; }
    public int TotalVolume { get; set; }
    public string PrimaryPeakHour { get; set; }
    public double PrimaryKFactor { get; set; }
    public int PrimaryPeakHourVolume { get; set; }
    public double PrimaryPeakHourFactor { get; set; }
    public int PrimaryTotalVolume { get; set; }
    public double PrimaryDFactor { get; set; }
    public string OpposingPeakHour { get; set; }
    public double OpposingKFactor { get; set; }
    public int OpposingPeakHourVolume { get; set; }
    public double OpposingPeakHourFactor { get; set; }
    public int OpposingTotalVolume { get; set; }
    public double OpposingDFactor { get; set; }
}
