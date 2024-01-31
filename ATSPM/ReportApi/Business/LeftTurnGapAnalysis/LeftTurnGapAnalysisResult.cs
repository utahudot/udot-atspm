using ATSPM.ReportApi.Business.Common;

namespace ATSPM.ReportApi.Business.LeftTurnGapAnalysis
{
    /// <summary>
    /// Left Turn Gap Analysis chart
    /// </summary>
    public class LeftTurnGapAnalysisResult : ApproachResult
    {
        public LeftTurnGapAnalysisResult(
            string locationId,
            int approachId,
            int phaseNumber,
            string phaseDescription,
            DateTime start,
            DateTime end,
            string detectionTypeDescription,
            double gap1Min,
            double gap1Max,
            ICollection<DataPointForInt> gap1Count,
            double gap2Min,
            double gap2Max,
            ICollection<DataPointForInt> gap2Count,
            double gap3Min,
            double gap3Max,
            ICollection<DataPointForInt> gap3Count,
            double? gap4Min,
            //double? gap4Max,
            //ICollection<DataPointForInt> gap4Count,
            //double? gap5Min,
            //double? gap5Max,
            //ICollection<DataPointForInt> gap5Count,
            //double? gap6Min,
            //double? gap6Max,
            //ICollection<DataPointForInt> gap6Count,
            //double? gap7Min,
            //double? gap7Max,
            //ICollection<DataPointForInt> gap7Count,
            //double? gap8Min,
            //double? gap8Max,
            //ICollection<DataPointForInt> gap8Count,
            //double? gap9Min,
            //double? gap9Max,
            //ICollection<DataPointForInt> gap9Count,
            //double? gap10Min,
            //double? gap10Max,
            //ICollection<DataPointForInt> gap10Count,
            ICollection<DataPointForDouble> percentTurnableSeries,
            double? sumDuration1,
            double? sumDuration2,
            double? sumDuration3,
            double sumGreenTime,
            int highestTotal,
            string detectionTypeStr,
            int binSize,
            double trendLineGapThreshold) : base(approachId, locationId, start, end)
        {
            PhaseNumber = phaseNumber;
            PhaseDescription = phaseDescription;
            DetectionTypeDescription = detectionTypeDescription;
            Gap1Min = gap1Min;
            Gap1Max = gap1Max;
            Gap1Count = gap1Count;
            Gap2Min = gap2Min;
            Gap2Max = gap2Max;
            Gap2Count = gap2Count;
            Gap3Min = gap3Min;
            Gap3Max = gap3Max;
            Gap3Count = gap3Count;
            Gap4Min = gap4Min;
            //Gap4Max = gap4Max;
            //Gap4Count = gap4Count;
            //Gap5Min = gap5Min;
            //Gap5Max = gap5Max;
            //Gap5Count = gap5Count;
            //Gap6Min = gap6Min;
            //Gap6Max = gap6Max;
            //Gap6Count = gap6Count;
            //Gap7Min = gap7Min;
            //Gap7Max = gap7Max;
            //Gap7Count = gap7Count;
            //Gap8Min = gap8Min;
            //Gap8Max = gap8Max;
            //Gap8Count = gap8Count;
            //Gap9Min = gap9Min;
            //Gap9Max = gap9Max;
            //Gap9Count = gap9Count;
            //Gap10Min = gap10Min;
            //Gap10Max = gap10Max;
            //Gap10Count = gap10Count;
            PercentTurnableSeries = percentTurnableSeries;
            SumDuration1 = sumDuration1;
            SumDuration2 = sumDuration2;
            SumDuration3 = sumDuration3;
            SumGreenTime = sumGreenTime;
            HighestTotal = highestTotal;
            DetectionTypeStr = detectionTypeStr;
            BinSize = binSize;
            TrendLineGapThreshold = trendLineGapThreshold;
        }
        public int PhaseNumber { get; internal set; }
        public string PhaseDescription { get; internal set; }
        public string DetectionTypeDescription { get; internal set; }
        public double Gap1Min { get; internal set; }
        public double Gap1Max { get; internal set; }
        public ICollection<DataPointForInt> Gap1Count { get; internal set; }
        public double Gap2Min { get; internal set; }
        public double Gap2Max { get; internal set; }
        public ICollection<DataPointForInt> Gap2Count { get; internal set; }
        public double Gap3Min { get; internal set; }
        public double Gap3Max { get; internal set; }
        public ICollection<DataPointForInt> Gap3Count { get; internal set; }
        public double? Gap4Min { get; internal set; }
        public double? Gap4Max { get; internal set; }
        public ICollection<DataPointForInt> Gap4Count { get; internal set; }
        public double? Gap5Min { get; internal set; }
        public double? Gap5Max { get; internal set; }
        public ICollection<DataPointForInt> Gap5Count { get; internal set; }
        public double? Gap6Min { get; internal set; }
        public double? Gap6Max { get; internal set; }
        public ICollection<DataPointForInt> Gap6Count { get; internal set; }
        public double? Gap7Min { get; internal set; }
        public double? Gap7Max { get; internal set; }
        public ICollection<DataPointForInt> Gap7Count { get; internal set; }
        public double? Gap8Min { get; internal set; }
        public double? Gap8Max { get; internal set; }
        public ICollection<DataPointForInt> Gap8Count { get; internal set; }
        public double? Gap9Min { get; internal set; }
        public double? Gap9Max { get; internal set; }
        public ICollection<DataPointForInt> Gap9Count { get; internal set; }
        public double? Gap10Min { get; internal set; }
        public double? Gap10Max { get; internal set; }
        public ICollection<DataPointForInt> Gap10Count { get; internal set; }
        public double? Gap11Min { get; internal set; }
        public double? Gap11Max { get; internal set; }
        public ICollection<DataPointForInt> Gap11Count { get; internal set; }
        public double? SumDuration1 { get; private set; }
        public double? SumDuration2 { get; private set; }
        public double? SumDuration3 { get; private set; }
        public double SumGreenTime { get; private set; }
        public int HighestTotal { get; set; }
        public string DetectionTypeStr { get; set; }
        public double TrendLineGapThreshold { get; set; }
        public int BinSize { get; set; }
        public ICollection<DataPointForDouble> PercentTurnableSeries { get; internal set; }


    }
}