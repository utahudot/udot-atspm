using ATSPM.ReportApi.Business.Common;

namespace ATSPM.ReportApi.Business.LeftTurnGapAnalysis
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
        public double TrendLineGapThreshold { get; set; } = 7.4;
        public int BinSize { get; set; }


    }
}
