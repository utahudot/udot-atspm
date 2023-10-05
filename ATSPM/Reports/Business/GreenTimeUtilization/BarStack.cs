using Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.PerdueCoordinationDiagram
{
    public class BarStack : DataPointBase
    {
        public List<Layer> Layers { get; }


        public BarStack(DateTime startAggTime, List<int> binValueList, int cycleCount, int binSize) : base(startAggTime)
        {
            //find the max layers number that is used
            int maxI = 0;
            for (int i = 0; i < binValueList.Count; i++)
            {
                if (binValueList[i] != 0 && i > maxI)
                {
                    maxI = i;
                }
            }
            //create Layers
            Layers = new List<Layer>();
            int binStart = 0;
            for (int i = 0; i <= maxI; i++)
            {
                Layers.Add(new Layer(binValueList[i], cycleCount, binStart));
                binStart = binStart + binSize;
            }
        }
    }
}