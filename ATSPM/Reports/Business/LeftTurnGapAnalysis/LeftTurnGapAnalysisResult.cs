using Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.LeftTurnGapAnalysis
{
    /// <summary>
    /// Left Turn Gap Analysis chart
    /// </summary>
    public class LeftTurnGapAnalysisResult:ApproachResult
    {
        public LeftTurnGapAnalysisResult(
            string signalId,
            int approachId,
            int phaseNumber,
            string phaseDescription,
            DateTime start,
            DateTime end,
            string detectionTypeDescription,
            double gap1,
            ICollection<GapCount> gap1Count,
            double gap2,
            ICollection<GapCount> gap2Count,
            double gap3,
            ICollection<GapCount> gap3Count,
            double? gap4,
            ICollection<GapCount> gap4Count,
            double? gap5,
            ICollection<GapCount> gap5Count,
            double? gap6,
            ICollection<GapCount> gap6Count,
            double? gap7,
            ICollection<GapCount> gap7Count,
            double? gap8,
            ICollection<GapCount> gap8Count,
            double? gap9,
            ICollection<GapCount> gap9Count,
            double? gap10,
            ICollection<GapCount> gap10Count,
            ICollection<PercentTurnableSeries> percentTurnableSeries,
            double? sumDuration1,
            double? sumDuration2,
            double? sumDuration3,
            double sumGreenTime,
            int highestTotal,
            string detectionTypeStr):base(approachId, signalId,  start, end)
        {
            PhaseNumber = phaseNumber;
            PhaseDescription = phaseDescription;
            DetectionTypeDescription = detectionTypeDescription;
            Gap1 = gap1;
            Gap1Count = gap1Count;
            Gap2 = gap2;
            Gap2Count = gap2Count;
            Gap3 = gap3;
            Gap3Count = gap3Count;
            Gap4 = gap4;
            Gap4Count = gap4Count;
            Gap5 = gap5;
            Gap5Count = gap5Count;
            Gap6 = gap6;
            Gap6Count = gap6Count;
            Gap7 = gap7;
            Gap7Count = gap7Count;
            Gap8 = gap8;
            Gap8Count = gap8Count;
            Gap9 = gap9;
            Gap9Count = gap9Count;
            Gap10 = gap10;
            Gap10Count = gap10Count;
            PercentTurnableSeries = percentTurnableSeries;
            SumDuration1 = sumDuration1;
            SumDuration2 = sumDuration2;
            SumDuration3 = sumDuration3;
            SumGreenTime = sumGreenTime;
            HighestTotal = highestTotal;
            DetectionTypeStr = detectionTypeStr;
        }
        public int PhaseNumber { get; internal set; }
        public string PhaseDescription { get; internal set; }
        public string DetectionTypeDescription { get; internal set; }
        public double Gap1 { get; internal set; }
        public ICollection<GapCount> Gap1Count { get; internal set; }
        public double Gap2 { get; internal set; }
        public ICollection<GapCount> Gap2Count { get; internal set; }
        public double Gap3 { get; internal set; }
        public ICollection<GapCount> Gap3Count { get; internal set; }
        public double? Gap4 { get; internal set; }
        public ICollection<GapCount> Gap4Count { get; internal set; }
        public double? Gap5 { get; internal set; }
        public ICollection<GapCount> Gap5Count { get; internal set; }
        public double? Gap6 { get; internal set; }
        public ICollection<GapCount> Gap6Count { get; internal set; }
        public double? Gap7 { get; internal set; }
        public ICollection<GapCount> Gap7Count { get; internal set; }
        public double? Gap8 { get; internal set; }
        public ICollection<GapCount> Gap8Count { get; internal set; }
        public double? Gap9 { get; internal set; }
        public ICollection<GapCount> Gap9Count { get; internal set; }
        public double? Gap10 { get; internal set; }
        public ICollection<GapCount> Gap10Count { get; internal set; }
        public double? Gap11 { get; internal set; }
        public ICollection<GapCount> Gap11Count { get; internal set; }
        public double? SumDuration1 { get; private set; }
        public double? SumDuration2 { get; private set; }
        public double? SumDuration3 { get; private set; }
        public double SumGreenTime { get; private set; }
        public int HighestTotal { get; set; }
        public string DetectionTypeStr { get; set; }
        public ICollection<PercentTurnableSeries> PercentTurnableSeries { get; internal set; }


    }
}