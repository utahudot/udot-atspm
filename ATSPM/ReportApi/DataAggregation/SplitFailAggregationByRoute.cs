#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - MOE.Common.Business.DataAggregation/SplitFailAggregationByRoute.cs
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
using ATSPM.Data.Models.AggregationModels;
using MOE.Common.Business.WCFServiceLibrary;

namespace MOE.Common.Business.DataAggregation
{
    class SplitFailAggregationByRoute
    {
        public SplitFailAggregationByRoute(ApproachAggregationMetricOptions options)
        {
            Container = new List<SplitFailAggregationBySignal>();

            //foreach (var sig in options.Signals)
            //{
            //    options.SignalID = sig.SignalID;
            //    var locationRepository = MOE.Common.Models.Repositories.SignalsRepositoryFactory.Create();
            //    Location signal = locationRepository.GetVersionOfSignalByDate(sig.SignalID, options.Start);

            //    SpliFailAggregationBySignal signalAggregation = new SpliFailAggregationBySignal(options);

            //    Container.Add(signalAggregation);
            //}

        }

        public List<SplitFailAggregationBySignal> Container { get; }


    }


    public class RouteSplitFailAggregationContainer
    {
        public Route Route { get; set; }
        public List<ApproachSplitFailAggregation> SplitFails { get; }

        public RouteSplitFailAggregationContainer(ApproachAggregationMetricOptions options)
        {
            //Approach = approach;
            //var splitFailAggregationRepository =
            //    MOE.Common.Models.Repositories.ApproachSplitFailAggregationRepositoryFactory.Create();
            //SplitFails = splitFailAggregationRepository.GetApproachSplitFailAggregationByApproachIdAndDateRange(
            //    approach.ApproachID, start, end);
        }
    }
}
