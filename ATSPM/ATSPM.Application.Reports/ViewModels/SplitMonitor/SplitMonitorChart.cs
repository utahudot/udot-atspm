using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.ViewModels.SplitMonitor;

/// <summary>
/// Split Monitor chart
/// </summary>
public class SplitMonitorChart
{
    public SplitMonitorChart(string chartName,
        string signalId,
        string signalLocation,
        int phaseNumber,
        string phaseDescription,
        DateTime start,
        DateTime end,
        ICollection<Plan> plans,
        ICollection<Split> splits,
        ICollection<SplitMonitorGapOut> gapOuts,
        ICollection<SplitMonitorMaxOut> maxOuts,
        ICollection<SplitMonitorForceOff> forceOffs,
        ICollection<SplitMonitorUnknown> unknowns,
        ICollection<Peds> peds)
    {
        ChartName = chartName;
        SignalId = signalId;
        SignalLocation = signalLocation;
        PhaseNumber = phaseNumber;
        PhaseDescription = phaseDescription;
        Start = start;
        End = end;
        Plans = plans;
        Splits = splits;
        GapOuts = gapOuts;
        MaxOuts = maxOuts;
        ForceOffs = forceOffs;
        Unknowns = unknowns;
        Peds = peds;
    }

    public string ChartName { get; internal set; }
    public string SignalId { get; internal set; }
    public string SignalLocation { get; internal set; }
    public int PhaseNumber { get; internal set; }
    public string PhaseDescription { get; internal set; }
    public DateTime Start { get; internal set; }
    public DateTime End { get; internal set; }
    public ICollection<Plan> Plans { get; internal set; }
    public ICollection<Split> Splits { get; internal set; }
    public ICollection<SplitMonitorGapOut> GapOuts { get; internal set; }
    public ICollection<SplitMonitorMaxOut> MaxOuts { get; internal set; }
    public ICollection<SplitMonitorForceOff> ForceOffs { get; internal set; }
    public ICollection<SplitMonitorUnknown> Unknowns { get; internal set; }
    public ICollection<Peds> Peds { get; internal set; }
}