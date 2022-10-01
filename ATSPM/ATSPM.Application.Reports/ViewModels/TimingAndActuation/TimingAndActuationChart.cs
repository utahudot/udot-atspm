using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.ViewModels.TimingAndActuation;

/// <summary>
/// Timing and Actuation chart
/// </summary>
public class TimingAndActuationChart
{
    public TimingAndActuationChart(string chartName, string signalId, string signalLocation, DateTime start, DateTime end, ICollection<EventType> eventTypes)
    {
        ChartName = chartName;
        SignalId = signalId;
        SignalLocation = signalLocation;
        Start = start;
        End = end;
        EventTypes = eventTypes;
    }

    public string ChartName { get; internal set; }
    public string SignalId { get; internal set; }
    public string SignalLocation { get; internal set; }
    public DateTime Start { get; internal set; }
    public DateTime End { get; internal set; }
    public System.Collections.Generic.ICollection<EventType> EventTypes { get; internal set; }
}
