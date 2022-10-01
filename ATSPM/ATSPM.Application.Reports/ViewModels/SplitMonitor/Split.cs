using System;

namespace ATSPM.Application.Reports.ViewModels.SplitMonitor;

public class Split
{
    public Split(DateTime startTime, DateTime endTime, int phaseNumber)
    {
        StartTime = startTime;
        EndTime = endTime;
        PhaseNumber = phaseNumber;
    }

    public DateTime StartTime { get; internal set; }
    public DateTime EndTime { get; internal set; }
    public int PhaseNumber { get; internal set; }
}
