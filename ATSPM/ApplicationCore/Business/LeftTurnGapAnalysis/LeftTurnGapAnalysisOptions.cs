using ATSPM.Application.Business.Common;

namespace ATSPM.Application.Business.LeftTurnGapAnalysis
{
    public class LeftTurnGapAnalysisOptions : OptionsBase
    {
        public const int EVENT_GREEN = 1;
        public const int EVENT_RED = 10;
        public const int EVENT_DET = 81;



        public LeftTurnGapAnalysisOptions()
        {
        }

        public double Gap1Min { get; set; } = 0;
        public double Gap1Max { get; set; } = 1;
        public double Gap2Min { get; set; } = 1;
        public double Gap2Max { get; set; } = 3.3;
        public double Gap3Min { get; set; } = 3.3;
        public double Gap3Max { get; set; } = 3.7;
        public double Gap4Min { get; set; } = 3.7;
        public double? Gap4Max { get; set; }// = 3.9;
        public double? Gap5Min { get; set; }// = 3.9;
        public double? Gap5Max { get; set; }// = 4.1;
        public double? Gap6Min { get; set; }// = 4.1;
        public double? Gap6Max { get; set; }// = 5.3;
        public double? Gap7Min { get; set; }// = 5.3;
        public double? Gap7Max { get; set; }// = 5.5;
        public double? Gap8Min { get; set; }// = 5.5;
        public double? Gap8Max { get; set; }// = 6.5;
        public double? Gap9Min { get; set; }// = 6.5;
        public double? Gap9Max { get; set; }// = 6.9;
        public double? Gap10Min { get; set; }// = 6.9;
        public double? Gap10Max { get; set; }// = 7.4;
        public double? SumDurationGap1 { get; set; }// = 4.1;
        public double? SumDurationGap2 { get; set; }// = 5.3;
        public double? SumDurationGap3 { get; set; }// = 7.4;
        public double TrendLineGapThreshold { get; set; } = 7.4;
        public int BinSize { get; set; }


    }
}
