#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - MOE.Common.Business.DataAggregation/AggregationByPhase.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using ATSPM.Application.Business.Aggregation;
using ATSPM.Application.Business.Bins;
using ATSPM.Data.Models;
using MOE.Common.Business.WCFServiceLibrary;

namespace MOE.Common.Business.DataAggregation
{
    public abstract class AggregationByPhase
    {
        //protected List<ApproachEventCountAggregation> ApproachEventCountAggregations { get; set; }
        public double Total
        {
            get { return BinsContainers.Sum(c => c.SumValue); }
        }


        public int PhaseNumber { get; }
        public List<BinsContainer> BinsContainers { get; set; } = new List<BinsContainer>();

        public int Average
        {
            get
            {
                if (BinsContainers.Count > 1)
                    return Convert.ToInt32(Math.Round(BinsContainers.Average(b => b.SumValue)));
                double numberOfBins = 0;
                foreach (var binsContainer in BinsContainers)
                    numberOfBins += binsContainer.Bins.Count;
                return numberOfBins > 0 ? Convert.ToInt32(Math.Round(Total / numberOfBins)) : 0;
            }
        }

        public AggregationByPhase(Location signal, int phaseNumber, PhaseAggregationMetricOptions phaseAggregationMetricOptions, int dataType, AggregationOptions options)
        {
            BinsContainers = BinFactory.GetBins(options.TimeOptions);
            PhaseNumber = phaseNumber;
            LoadBins(signal, phaseNumber, phaseAggregationMetricOptions, dataType, options);
        }


        protected abstract void LoadBins(Location signal, int phaseNumber, PhaseAggregationMetricOptions phaseAggregationMetricOptions, int dataType, AggregationOptions options);


    }
}