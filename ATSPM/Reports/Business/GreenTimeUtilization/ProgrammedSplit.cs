using ATSPM.Application.Reports.Business.Common;
using Reports.Business.Common;
using System;

namespace ATSPM.Application.Reports.Business.PerdueCoordinationDiagram
{
    public class ProgrammedSplit : DataPointBase
    {
        public double ProgValue { get; set; }


        public ProgrammedSplit(Plan analysisPlan, DateTime analysisStart, double splitLength, double durYR) : base(analysisPlan.Start)
        {
            if (analysisStart < analysisPlan.Start)
            {
                Timestamp = analysisPlan.Start;
            }
            else
            {
                Timestamp = analysisStart;
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