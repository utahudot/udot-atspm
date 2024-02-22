using System;

namespace ATSPM.ReportApi.Business.SplitMonitor;

public class Split
{
    public Split(DateTime time, int phaseNumber)
    {
        Time = time;
        PhaseNumber = phaseNumber;
    }

    public DateTime Time { get; internal set; }
    public int PhaseNumber { get; internal set; }
}
