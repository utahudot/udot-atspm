using ATSPM.ReportApi.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.ReportApi.Business.PurdueCoordinationDiagram;

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
        ICollection<DataPointForDouble> volumePerHour,
        ICollection<DataPointForDouble> redSeries,
        ICollection<DataPointForDouble> yellowSeries,
        ICollection<DataPointForDouble> greenSeries,
        ICollection<DataPointForDouble> detectorEvents) : base(approachId, signalId, start, end)
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
    public ICollection<DataPointForDouble> VolumePerHour { get; }
    public ICollection<DataPointForDouble> RedSeries { get; }
    public ICollection<DataPointForDouble> YellowSeries { get; }
    public ICollection<DataPointForDouble> GreenSeries { get; }
    public ICollection<DataPointForDouble> DetectorEvents { get; }

}