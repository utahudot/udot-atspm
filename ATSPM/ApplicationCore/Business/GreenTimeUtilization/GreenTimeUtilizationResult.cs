using ATSPM.Application.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.GreenTimeUtilization
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
            int XAxisBinSize, List<PlanSplitMonitorData> plans) : base(approachId, locationIdentifier, start, end)
        {
            Bins = stacks;
            AverageSplits = avgSplits;
            ProgrammedSplits = progSplits;
            PhaseNumber = phaseNumber;
            this.YAxisBinSize = YAxisBinSize;
            this.XAxisBinSize = XAxisBinSize;
            Plans = plans;
        }

        public List<BarStack> Bins { get; set; } = new List<BarStack>();
        public List<DataPointForDouble> AverageSplits { get; set; } = new List<DataPointForDouble>();
        public List<DataPointForDouble> ProgrammedSplits { get; set; } = new List<DataPointForDouble>();
        public int PhaseNumber { get; set; }
        public int YAxisBinSize { get; set; }
        public int XAxisBinSize { get; set; }
        public List<PlanSplitMonitorData> Plans { get; }
    }
}