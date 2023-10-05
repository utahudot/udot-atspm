using ATSPM.Data.Enums;
using Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.ApproachVolume;

/// <summary>
/// Approach Volume Chart
/// </summary>
public class ApproachVolumeResult : SignalResult
{
    public ApproachVolumeResult(
        string signalId,
        DateTime start,
        DateTime end,
        DirectionTypes directionType) : base(signalId, start, end)
    {
        this.PrimaryDirectionName = directionType.ToString();
    }

    public ApproachVolumeResult(
        string signalIdentifier,
        DateTime start,
        DateTime end,
        string detectorType,
        int distanceFromStopBar,
        string primaryDirectionName,
        ICollection<DataPointForInt> primaryDirectionVolumes,
        string opposingDirectionName,
        ICollection<DataPointForInt> opposingDirectionVolumes,
        ICollection<DataPointForInt> combinedDirectionVolumes,
        ICollection<DFactors> primaryDFactors,
        ICollection<DFactors> opposingDFactors,
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
        string opposingPeakHour,
        double opposingKFactor,
        int opposingPeakHourVolume,
        double opposingPeakHourFactor,
        int opposingTotalVolume) : base(signalIdentifier, start, end)
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
        opposingPeakHour,
        opposingKFactor,
        opposingPeakHourVolume,
        opposingPeakHourFactor,
        opposingTotalVolume);
    }

    public string PrimaryDirectionName { get; set; }
    public string OpposingDirectionName { get; set; }
    public int DistanceFromStopBar { get; set; }
    public string DetectorType { get; set; }
    public ICollection<DataPointForInt> PrimaryDirectionVolumes { get; set; }
    public ICollection<DataPointForInt> OpposingDirectionVolumes { get; set; }
    public ICollection<DataPointForInt> CombinedDirectionVolumes { get; set; }
    public ICollection<DFactors> PrimaryDFactors { get; set; }
    public ICollection<DFactors> OpposingDFactors { get; set; }
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
        string opposingPeakHour,
        double opposingKFactor,
        int opposingPeakHourVolume,
        double opposingPeakHourFactor,
        int opposingTotalVolume)
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
        OpposingPeakHour = opposingPeakHour;
        OpposingKFactor = opposingKFactor;
        OpposingPeakHourVolume = opposingPeakHourVolume;
        OpposingPeakHourFactor = opposingPeakHourFactor;
        OpposingTotalVolume = opposingTotalVolume;
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
    public string OpposingPeakHour { get; set; }
    public double OpposingKFactor { get; set; }
    public int OpposingPeakHourVolume { get; set; }
    public double OpposingPeakHourFactor { get; set; }
    public int OpposingTotalVolume { get; set; }
}
