using System;
using System.Collections.Generic;

namespace ATSPM.ReportApi.Business.PhaseTermination;

public class Phase
{
    public Phase(
        int phaseNumber,
        ICollection<DateTime> gapOuts,
        ICollection<DateTime> maxOuts,
        ICollection<DateTime> forceOffs,
        ICollection<DateTime> pedWalkBegins,
        ICollection<DateTime> unknownTerminations)
    {
        PhaseNumber = phaseNumber;
        GapOuts = gapOuts;
        MaxOuts = maxOuts;
        ForceOffs = forceOffs;
        PedWalkBegins = pedWalkBegins;
        UnknownTerminations = unknownTerminations;
    }

    public int PhaseNumber { get; set; }
    public ICollection<DateTime> GapOuts { get; internal set; }
    public ICollection<DateTime> MaxOuts { get; internal set; }
    public ICollection<DateTime> ForceOffs { get; internal set; }
    public ICollection<DateTime> PedWalkBegins { get; internal set; }
    public ICollection<DateTime> UnknownTerminations { get; internal set; }
}
