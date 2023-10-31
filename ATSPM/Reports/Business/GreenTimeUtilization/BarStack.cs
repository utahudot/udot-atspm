using Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.PerdueCoordinationDiagram
{
    public class BarStack
    {
        public int X { get; set; }
        public int Y { get; set; }

        public double Value { get; set; }

        public BarStack(int xAxisBinNumber, int yAxisBinNumber, double value)
        {
            X = xAxisBinNumber;
            Y = yAxisBinNumber;
            Value = value;
        }
    }
}