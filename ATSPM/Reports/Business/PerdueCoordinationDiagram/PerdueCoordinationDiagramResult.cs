using ATSPM.Application.Reports.Business.Common;
using Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.PerdueCoordinationDiagram;

/// <summary>
/// Perdue Coordination Diagram chart
/// </summary>
public class PerdueCoordinationDiagramResult : ApproachResult
{
    public PerdueCoordinationDiagramResult(
        string signalId,
        int approachId,
        int phaseNumber,
        string phaseDescription,
        DateTime start,
        DateTime end,
        int totalOnGreenEvents,
        int totalDetectorHits,
        double percentArrivalOnGreen,
        ICollection<PerdueCoordinationPlan> plans,
        ICollection<VolumePerHour> volumePerHour,
        ICollection<CyclePcd> cycles) : base(approachId, signalId, start, end)
    {
        PhaseNumber = phaseNumber;
        PhaseDescription = phaseDescription;
        TotalOnGreenEvents = totalOnGreenEvents;
        TotalDetectorHits = totalDetectorHits;
        PercentArrivalOnGreen = percentArrivalOnGreen;
        Plans = plans;
        VolumePerHour = volumePerHour;
        Cycles = cycles;
    }
    public int PhaseNumber { get; }
    public string PhaseDescription { get; }
    public int TotalOnGreenEvents { get; }
    public int TotalDetectorHits { get; }
    public double PercentArrivalOnGreen { get; }
    public ICollection<PerdueCoordinationPlan> Plans { get; }
    public ICollection<VolumePerHour> VolumePerHour { get; }
    public ICollection<CyclePcd> Cycles { get; }

}
