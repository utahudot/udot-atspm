using System;

namespace ATSPM.Application.Reports.ViewModels.PhaseTerminationChart;

public class UnknownTermination
{
    public UnknownTermination(DateTime startTime, int phaseNumber)
    {
        StartTime = startTime;
        PhaseNumber = phaseNumber;
    }

    public DateTime StartTime { get; internal set; }
    public int PhaseNumber { get; internal set; }
}