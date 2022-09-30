using System;

namespace ATSPM.Application.Reports.ViewModels.PhaseTerminationChart;

public class PedWalkBegin
{
    public PedWalkBegin(DateTime startTime, double phaseNumber)
    {
        StartTime = startTime;
        PhaseNumber = phaseNumber;
    }

    public DateTime StartTime { get; internal set; }
    public double PhaseNumber { get; internal set; }
}
