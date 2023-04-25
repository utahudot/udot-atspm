using System;

namespace ATSPM.Application.Reports.Business.PhaseTermination;

public class ForceOff
{
    public ForceOff(DateTime startTime, int phaseNumber)
    {
        StartTime = startTime;
        PhaseNumber = phaseNumber;
    }

    public DateTime StartTime { get; internal set; }
    public int PhaseNumber { get; internal set; }
}
