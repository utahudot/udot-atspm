using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.ViewModels.PerdueCoordinationDiagram;

/// <summary>
/// Perdue Coordination Diagram chart
/// </summary>
public class PerdueCoordinationDiagramChart
{
    public PerdueCoordinationDiagramChart(
        string chartName,
        string signalId,
        string signalLocation,
        int phaseNumber,
        string phaseDescription,
        DateTime start,
        DateTime end,
        int totalOnGreenEvents,
        int totalDetectorHits,
        double percentArrivalOnGreen,
        ICollection<PerdueCoordinationPlan> plans,
        ICollection<VolumePerHour> volumePerHour,
        ICollection<GreenEvent> greenEvents,
        ICollection<YellowEvent> yellowEvents,
        ICollection<RedEvent> redEvents,
        ICollection<DetectorActivation> detectorActivations)
    {
        ChartName = chartName;
        SignalId = signalId;
        SignalLocation = signalLocation;
        PhaseNumber = phaseNumber;
        PhaseDescription = phaseDescription;
        Start = start;
        End = end;
        TotalOnGreenEvents = totalOnGreenEvents;
        TotalDetectorHits = totalDetectorHits;
        PercentArrivalOnGreen = percentArrivalOnGreen;
        Plans = plans;
        VolumePerHour = volumePerHour;
        GreenEvents = greenEvents;
        YellowEvents = yellowEvents;
        RedEvents = redEvents;
        DetectorActivations = detectorActivations;
    }

    public string ChartName { get; internal set; }
    public string SignalId { get; internal set; }
    public string SignalLocation { get; internal set; }
    public int PhaseNumber { get; internal set; }
    public string PhaseDescription { get; internal set; }
    public DateTime Start { get; internal set; }
    public DateTime End { get; internal set; }
    public int TotalOnGreenEvents { get; internal set; }
    public int TotalDetectorHits { get; internal set; }
    public double PercentArrivalOnGreen { get; internal set; }
    public ICollection<PerdueCoordinationPlan> Plans { get; internal set; }
    public ICollection<VolumePerHour> VolumePerHour { get; internal set; }
    public ICollection<GreenEvent> GreenEvents { get; internal set; }
    public ICollection<YellowEvent> YellowEvents { get; internal set; }
    public ICollection<RedEvent> RedEvents { get; internal set; }
    public ICollection<DetectorActivation> DetectorActivations { get; internal set; }

}
