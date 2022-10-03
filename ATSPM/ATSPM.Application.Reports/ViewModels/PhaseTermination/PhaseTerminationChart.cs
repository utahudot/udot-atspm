using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.ViewModels.PhaseTerminationChart;

/// <summary>
/// Phase Termination chart
/// </summary>
public class PhaseTerminationChart
{
    public PhaseTerminationChart(
        string chartName,
        string signalId,
        string signalLocation,
        int phaseNumber,
        string phaseDescription,
        DateTime start,
        DateTime end,
        int consecutiveCount,
        ICollection<Plan> plans,
        ICollection<GapOut> gapOuts,
        ICollection<MaxOut> maxOuts,
        ICollection<ForceOff> forceOffs,
        ICollection<PedWalkBegin> pedWalkBegins,
        ICollection<UnknownTermination> unknownTerminations)
    {
        ChartName = chartName;
        SignalId = signalId;
        SignalLocation = signalLocation;
        PhaseNumber = phaseNumber;
        PhaseDescription = phaseDescription;
        Start = start;
        End = end;
        ConsecutiveCount = consecutiveCount;
        Plans = plans;
        GapOuts = gapOuts;
        MaxOuts = maxOuts;
        ForceOffs = forceOffs;
        PedWalkBegins = pedWalkBegins;
        UnknownTerminations = unknownTerminations;
    }

    public string ChartName { get; internal set; }
    public string SignalId { get; internal set; }
    public string SignalLocation { get; internal set; }
    public int PhaseNumber { get; internal set; }
    public string PhaseDescription { get; internal set; }
    public DateTime Start { get; internal set; }
    public DateTime End { get; internal set; }
    public int ConsecutiveCount { get; internal set; }
    public ICollection<Plan> Plans { get; internal set; }
    public ICollection<GapOut> GapOuts { get; internal set; }
    public ICollection<MaxOut> MaxOuts { get; internal set; }
    public ICollection<ForceOff> ForceOffs { get; internal set; }
    public ICollection<PedWalkBegin> PedWalkBegins { get; internal set; }
    public ICollection<UnknownTermination> UnknownTerminations { get; internal set; }
}
