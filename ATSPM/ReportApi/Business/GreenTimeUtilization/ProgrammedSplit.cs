﻿using ATSPM.ReportApi.Business.Common;
using System;

namespace ATSPM.ReportApi.Business.GreenTimeUtilization
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