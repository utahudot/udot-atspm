using ATSPM.Application.Reports.Business.Common;
using System;

namespace ATSPM.Application.Reports.Business.PerdueCoordinationDiagram
{
    public class ProgrammedSplit
    {
        public DateTime StartTime { get; set; }
        public double ProgValue { get; set; }


        public ProgrammedSplit(Plan analysisPlan, DateTime analysisStart, double splitLength, double durYR)
        {
            if (analysisStart < analysisPlan.Start)
            {
                StartTime = analysisPlan.Start;
            }
            else
            {
                StartTime = analysisStart;
            }

            if (splitLength >= durYR)
            {
                ProgValue = splitLength - durYR;
            }
            else
            {
                ProgValue = 0;
            }
        }

    }
}