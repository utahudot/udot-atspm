using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Reports.Business.PerdueCoordinationDiagram
{
    public class AverageSplit
    {
        public DateTime StartTime { get; set; }
        public double AvgValue { get; set; }


        public AverageSplit(DateTime startAggTime, List<double> greenDurationList)
        {
            AvgValue = greenDurationList.Average();
            StartTime = startAggTime;
        }

    }
}