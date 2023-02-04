
using ATSPM.Data.Enums;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ATSPM.Application.Reports.ViewModels.ApproachVolume;

/// <summary>
/// Approach Volume Chart
/// </summary>
public class ApproachVolumeResult
{
    public ApproachVolumeResult(
        string chartName,
        string signalId,
        string signalLocation,
        DateTime start,
        DateTime end,
        DetectionTypes detectorType,
        int distanceFromStopBar,
        string primaryDirectionName,
        ICollection<DirectionVolumes> primaryDirectionVolumes,
        string opposingDirectionName,
        ICollection<DirectionVolumes> opposingDirectionVolumes,
        ICollection<DirectionVolumes> combinedDirectionVolumes,
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
        int opposingTotalVolume)
    {
        ChartName = chartName;
        SignalId = signalId;
        SignalLocation = signalLocation;
        Start = start;
        End = end;
        DetectorType = detectorType;
        DistanceFromStopBar = distanceFromStopBar;
        PrimaryDirectionName = primaryDirectionName;
        PrimaryDirectionVolumes = primaryDirectionVolumes;
        OpposingDirectionName = opposingDirectionName;
        OpposingDirectionVolumes = opposingDirectionVolumes;
        CombinedDirectionVolumes = combinedDirectionVolumes;
        PrimaryDFactors = primaryDFactors;
        OpposingDFactors = opposingDFactors;
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

    public string ChartName { get; internal set; }
    public string SignalId { get; internal set; }
    public string SignalLocation { get; internal set; }
    public DateTime Start { get; internal set; }
    public DateTime End { get; internal set; }
    public DetectionTypes DetectorType { get; internal set; }
    public int DistanceFromStopBar { get; internal set; }
    public string PrimaryDirectionName { get; internal set; }
    public ICollection<DirectionVolumes> PrimaryDirectionVolumes { get; internal set; }
    public string OpposingDirectionName { get; set; }
    public ICollection<DirectionVolumes> OpposingDirectionVolumes { get; internal set; }
    public ICollection<DirectionVolumes> CombinedDirectionVolumes { get; internal set; }
    public ICollection<DFactors> PrimaryDFactors { get; internal set; }
    public ICollection<DFactors> OpposingDFactors { get; internal set; }
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
