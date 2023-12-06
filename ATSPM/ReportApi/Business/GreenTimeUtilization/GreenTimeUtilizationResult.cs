using ATSPM.ReportApi.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.ReportApi.Business.GreenTimeUtilization
{
    public class GreenTimeUtilizationResult : ApproachResult
    {
        public GreenTimeUtilizationResult(
            int approachId,
            string signalIdentifier,
            DateTime start,
            DateTime end,
            List<BarStack> stacks,
            List<DataPointForDouble> avgSplits,
            List<DataPointForDouble> progSplits,
            int phaseNumber,
            string phaseNumberSort) : base(approachId, signalIdentifier, start, end)
        {
            Bins = stacks;
            AvgSplits = avgSplits;
            ProgSplits = progSplits;
            PhaseNumber = phaseNumber;
            PhaseNumberSort = phaseNumberSort;
        }

        public List<BarStack> Bins { get; set; } = new List<BarStack>();
        public List<DataPointForDouble> AvgSplits { get; set; } = new List<DataPointForDouble>();
        public List<DataPointForDouble> ProgSplits { get; set; } = new List<DataPointForDouble>();
        public int PhaseNumber { get; set; }
        public string PhaseNumberSort { get; set; }
    }
}