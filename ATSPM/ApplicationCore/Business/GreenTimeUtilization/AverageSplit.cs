using ATSPM.Application.Business.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.GreenTimeUtilization
{
    public class AverageSplit : DataPointBase
    {
        public double AvgValue { get; set; }


        public AverageSplit(DateTime startAggTime, List<double> greenDurationList) : base(startAggTime)
        {
            AvgValue = greenDurationList.Average();
        }

    }
}