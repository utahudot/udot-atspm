using System;

namespace ATSPM.Application.Reports.ViewModels.TimingAndActuation;

/// <summary>
/// Timing and Actuation chart
/// </summary>
public class TimingAndActuationChart
{
    public string ChartName { get; set; }
    public string SignalId { get; set; }
    public string SignalLocation { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public System.Collections.Generic.ICollection<EventType> EventTypes { get; set; }
}
