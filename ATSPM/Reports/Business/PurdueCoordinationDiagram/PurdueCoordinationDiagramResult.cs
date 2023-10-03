using ATSPM.Application.Reports.Business.Common;
using Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.PerdueCoordinationDiagram;

/// <summary>
/// Perdue Coordination Diagram chart
/// </summary>
public class PurdueCoordinationDiagramResult : ApproachResult
{
    public PurdueCoordinationDiagramResult(
        string signalId,
        int approachId,
        int phaseNumber,
        string phaseDescription,
        DateTime start,
        DateTime end,
        int totalOnGreenEvents,
        int totalDetectorHits,
        double percentArrivalOnGreen,
        ICollection<PerdueCoordinationPlanViewModel> plans,
        ICollection<VolumePerHour> volumePerHour,
        ICollection<DataPointSeconds> redSeries,
        ICollection<DataPointSeconds> yellowSeries,
        ICollection<DataPointSeconds> greenSeries,
        ICollection<DataPointSeconds> detectorEvents) : base(approachId, signalId, start, end)
    {
        PhaseNumber = phaseNumber;
        PhaseDescription = phaseDescription;
        TotalOnGreenEvents = totalOnGreenEvents;
        TotalDetectorHits = totalDetectorHits;
        PercentArrivalOnGreen = percentArrivalOnGreen;
        Plans = plans;
        VolumePerHour = volumePerHour;
        RedSeries = redSeries;
        YellowSeries = yellowSeries;
        GreenSeries = greenSeries;
        DetectorEvents = detectorEvents;
    }
    public int PhaseNumber { get; }
    public string PhaseDescription { get; }
    public int TotalOnGreenEvents { get; }
    public int TotalDetectorHits { get; }
    public double PercentArrivalOnGreen { get; }
    public ICollection<PerdueCoordinationPlanViewModel> Plans { get; }
    public ICollection<VolumePerHour> VolumePerHour { get; }
    public ICollection<DataPointSeconds> RedSeries { get; }
    public ICollection<DataPointSeconds> YellowSeries { get; }
    public ICollection<DataPointSeconds> GreenSeries { get; }
    public ICollection<DataPointSeconds> DetectorEvents { get; }

}
