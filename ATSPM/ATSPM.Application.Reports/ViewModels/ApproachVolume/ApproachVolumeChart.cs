
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.ViewModels.ApproachVolume;

/// <summary>
/// Approach Volume Chart
/// </summary>
public class ApproachVolumeChart
{
    public ApproachVolumeChart(
        string chartName,
        string signalId,
        string signalLocation,
        int phaseNumber,
        string phaseDescription,
        DateTime start,
        DateTime end,
        string detectorType,
        int distanceFromStopBar,
        ICollection<ApproachVolumePlan> plans,
        string primaryDirectionName,
        ICollection<PrimaryDirectionVolumes> primaryDirectionVolumes,
        string opposingDirectionName,
        ICollection<OpposingDirectionVolumes> opposingDirectionVolumes,
        ICollection<CombinedDirectionVolumes> combinedDirectionVolumes,
        ICollection<PrimaryDFactors> primaryDFactors,
        ICollection<OpposingDFactors> opposingDFactors)
    {
        ChartName = chartName;
        SignalId = signalId;
        SignalLocation = signalLocation;
        PhaseNumber = phaseNumber;
        PhaseDescription = phaseDescription;
        Start = start;
        End = end;
        DetectorType = detectorType;
        DistanceFromStopBar = distanceFromStopBar;
        Plans = plans;
        PrimaryDirectionName = primaryDirectionName;
        PrimaryDirectionVolumes = primaryDirectionVolumes;
        OpposingDirectionName = opposingDirectionName;
        OpposingDirectionVolumes = opposingDirectionVolumes;
        CombinedDirectionVolumes = combinedDirectionVolumes;
        PrimaryDFactors = primaryDFactors;
        OpposingDFactors = opposingDFactors;
    }

    public string ChartName { get; internal set; }
    public string SignalId { get; internal set; }
    public string SignalLocation { get; internal set; }
    public int PhaseNumber { get; internal set; }
    public string PhaseDescription { get; internal set; }
    public DateTime Start { get; internal set; }
    public DateTime End { get; internal set; }
    public string DetectorType { get; internal set; }
    public int DistanceFromStopBar { get; internal set; }
    public ICollection<ApproachVolumePlan> Plans { get; internal set; }
    public string PrimaryDirectionName { get; internal set; }
    public ICollection<PrimaryDirectionVolumes> PrimaryDirectionVolumes { get; internal set; }
    public string OpposingDirectionName { get; set; }
    public ICollection<OpposingDirectionVolumes> OpposingDirectionVolumes { get; internal set; }
    public ICollection<CombinedDirectionVolumes> CombinedDirectionVolumes { get; internal set; }
    public ICollection<PrimaryDFactors> PrimaryDFactors { get; internal set; }
    public ICollection<OpposingDFactors> OpposingDFactors { get; internal set; }

}
