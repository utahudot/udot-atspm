using ATSPM.ReportApi.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.ReportApi.Business.GreenTimeUtilization
{
    public class GreenTimeUtilizationResult : ApproachResult
    {
        public GreenTimeUtilizationResult(
            int approachId,
            string locationIdentifier,
            DateTime start,
            DateTime end,
            List<BarStack> stacks,
            List<DataPointForDouble> avgSplits,
            List<DataPointForDouble> progSplits,
            int phaseNumber,
            int YAxisBinSize,
            int XAxisBinSize) : base(approachId, locationIdentifier, start, end)
        {
            Bins = stacks;
            AverageSplits = avgSplits;
            ProgrammedSplits = progSplits;
            PhaseNumber = phaseNumber;
            this.YAxisBinSize = YAxisBinSize;
            this.XAxisBinSize = XAxisBinSize;
        }

        public List<BarStack> Bins { get; set; } = new List<BarStack>();
        public List<DataPointForDouble> AverageSplits { get; set; } = new List<DataPointForDouble>();
        public List<DataPointForDouble> ProgrammedSplits { get; set; } = new List<DataPointForDouble>();
        public int PhaseNumber { get; set; }
        public int YAxisBinSize { get; set; }
        public int XAxisBinSize { get; set; }
    }
}