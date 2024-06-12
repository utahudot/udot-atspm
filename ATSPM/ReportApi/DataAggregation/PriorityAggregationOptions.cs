#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - MOE.Common.Business.WCFServiceLibrary/PriorityAggregationOptions.cs
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
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data.Models;
using MOE.Common.Business.DataAggregation;

namespace MOE.Common.Business.WCFServiceLibrary
{
    public enum PriorityDataTypes { PriorityNumber, PriorityRequests, PriorityServiceEarlyGreen, PriorityServiceExtendedGreen }

    //AggregatedDataTypes = new List<AggregatedDataType>
    //{
    //    new AggregatedDataType { Id = 0, DataName = "PriorityNumber" },
    //    new AggregatedDataType { Id = 1, DataName = "PriorityRequests" },
    //    new AggregatedDataType { Id = 2, DataName = "PriorityServiceEarlyGreen" },
    //    new AggregatedDataType { Id = 3, DataName = "PriorityServiceExtendedGreen" }
    //};

    public class PriorityAggregationOptions : SignalAggregationMetricOptions
    {
        private readonly IPriorityAggregationRepository priorityAggregationRepository;

        public PriorityAggregationOptions(
            IPriorityAggregationRepository priorityAggregationRepository,
            ILocationRepository locationRepository,
            ILogger<PriorityAggregationOptions> logger) : base(locationRepository, logger)
        {
            this.priorityAggregationRepository = priorityAggregationRepository;
        }

        //public override string YAxisTitle => SelectedAggregationType + " of " + Regex.Replace(
        //                                         SelectedAggregatedDataType.DataName,
        //                                         @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1") + " " +
        //                                     TimePeriodOptions.SelectedBinSize + " bins";

        protected override List<BinsContainer> GetBinsContainersBySignal(Location signal, AggregationOptions options)
        {
            var priorityAggregationBySignal = new PriorityAggregationBySignal(this, signal, priorityAggregationRepository, options);
            return priorityAggregationBySignal.BinsContainers;
        }

        public override List<BinsContainer> GetBinsContainersByRoute(List<Location> signals, AggregationOptions options)
        {
            var binsContainers = BinFactory.GetBins(options.TimeOptions);
            foreach (var signal in signals)
            {
                var priorityAggregationBySignal = new PriorityAggregationBySignal(this, signal, priorityAggregationRepository, options);
                PopulateBinsForRoute(signals, binsContainers, priorityAggregationBySignal);
            }
            return binsContainers;
        }
    }
}