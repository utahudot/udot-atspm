using Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.PerdueCoordinationDiagram
{
    public class GreenTimeUtilizationResult : ApproachResult
    {
        public GreenTimeUtilizationResult(
            int approachId,
            string signalIdentifier,
            DateTime start,
            DateTime end,
            List<BarStack> stacks,
            List<AverageSplit> avgSplits,
            List<ProgrammedSplit> progSplits,
            int phaseNumber,
            string phaseNumberSort) : base(approachId, signalIdentifier, start, end)
        {
            Stacks = stacks;
            AvgSplits = avgSplits;
            ProgSplits = progSplits;
            PhaseNumber = phaseNumber;
            PhaseNumberSort = phaseNumberSort;
        }

        public List<BarStack> Stacks { get; set; } = new List<BarStack>();
        public List<AverageSplit> AvgSplits { get; set; } = new List<AverageSplit>();
        public List<ProgrammedSplit> ProgSplits { get; set; } = new List<ProgrammedSplit>();
        public int PhaseNumber { get; set; }
        public string PhaseNumberSort { get; set; }
    }
}