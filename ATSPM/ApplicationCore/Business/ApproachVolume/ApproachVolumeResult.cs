using ATSPM.Data.Enums;
using ATSPM.ReportApi.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.ReportApi.Business.ApproachVolume;

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
        ICollection<DataPointForInt> primaryDirectionVolumes,
        string opposingDirectionName,
        ICollection<DataPointForInt> opposingDirectionVolumes,
        ICollection<DataPointForInt> combinedDirectionVolumes,
        ICollection<DataPointForDouble> primaryDFactors,
        ICollection<DataPointForDouble> opposingDFactors,
        string peakHour,
        double kFactor,
        int peakHourVolume,
        double peakHourFactor,
        int totalVolume,
        string primaryPeakHour,
        double primaryKFactor,
        int primaryPeakHourVolume,
        double primaryPeakHourFactor,
        int primaryTotalVolume,
        double primaryPeakHourDFactor,
        string opposingPeakHour,
        double opposingKFactor,
        int opposingPeakHourVolume,
        double opposingPeakHourFactor,
        int opposingTotalVolume,
        double opposingPeakHourDFactor) : base(locationIdentifier, start, end)
    {
        DetectorType = detectorType;
        DistanceFromStopBar = distanceFromStopBar;
        PrimaryDirectionName = primaryDirectionName;
        PrimaryDirectionVolumes = primaryDirectionVolumes;
        OpposingDirectionName = opposingDirectionName;
        OpposingDirectionVolumes = opposingDirectionVolumes;
        CombinedDirectionVolumes = combinedDirectionVolumes;
        PrimaryDFactors = primaryDFactors;
        OpposingDFactors = opposingDFactors;
        SummaryData = new SummaryData(
        peakHour,
        kFactor,
        peakHourVolume,
        peakHourFactor,
        totalVolume,
        primaryPeakHour,
        primaryKFactor,
        primaryPeakHourVolume,
        primaryPeakHourFactor,
        primaryTotalVolume,
        primaryPeakHourDFactor,
        opposingPeakHour,
        opposingKFactor,
        opposingPeakHourVolume,
        opposingPeakHourFactor,
        opposingTotalVolume,
        opposingPeakHourDFactor);
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
    public SummaryData(
        string peakHour,
        double kFactor,
        int peakHourVolume,
        double peakHourFactor,
        int totalVolume,
        string primaryPeakHour,
        double primaryKFactor,
        int primaryPeakHourVolume,
        double primaryPeakHourFactor,
        int primaryTotalVolume,
        double primaryPeakHourDFactor,
        string opposingPeakHour,
        double opposingKFactor,
        int opposingPeakHourVolume,
        double opposingPeakHourFactor,
        int opposingTotalVolume,
        double opposingPeakHourDFactor)
    {
        PeakHour = peakHour;
        KFactor = kFactor;
        PeakHourVolume = peakHourVolume;
        PeakHourFactor = peakHourFactor;
        TotalVolume = totalVolume;
        PrimaryPeakHour = primaryPeakHour;
        PrimaryKFactor = primaryKFactor;
        PrimaryPeakHourVolume = primaryPeakHourVolume;
        PrimaryPeakHourFactor = primaryPeakHourFactor;
        PrimaryTotalVolume = primaryTotalVolume;
        PrimaryDFactor = primaryPeakHourDFactor;
        OpposingPeakHour = opposingPeakHour;
        OpposingKFactor = opposingKFactor;
        OpposingPeakHourVolume = opposingPeakHourVolume;
        OpposingPeakHourFactor = opposingPeakHourFactor;
        OpposingTotalVolume = opposingTotalVolume;
        OpposingDFactor = opposingPeakHourDFactor;
    }

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
    public double PrimaryDFactor { get; }
    public string OpposingPeakHour { get; set; }
    public double OpposingKFactor { get; set; }
    public int OpposingPeakHourVolume { get; set; }
    public double OpposingPeakHourFactor { get; set; }
    public int OpposingTotalVolume { get; set; }
    public double OpposingDFactor { get; }
}
