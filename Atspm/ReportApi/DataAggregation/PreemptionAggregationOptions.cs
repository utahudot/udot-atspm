﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - ATSPM.ReportApi.DataAggregation/PreemptionAggregationOptions.cs
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


namespace Utah.Udot.Atspm.ReportApi.DataAggregation
{
    public enum PreemptionDataTypes
    {
        PreemptNumber,
        PreemptRequests,
        PreemptServices
    }
    //AggregatedDataTypes = new List<AggregatedDataType>
    //        {
    //            new AggregatedDataType { Id = 0, DataName = "PreemptNumber" },
    //            new AggregatedDataType { Id = 1, DataName = "PreemptRequests" },
    //            new AggregatedDataType { Id = 2, DataName = "PreemptServices" }
    //        };

    public class PreemptionAggregationOptions : SignalAggregationMetricOptions
    {
        private readonly IPreemptionAggregationRepository preemptionAggregationRepository;

        public PreemptionAggregationOptions(
            IPreemptionAggregationRepository preemptionAggregationRepository,
            ILocationRepository locationRepository,
            ILogger<PhaseTerminationAggregationOptions> logger) : base(locationRepository, logger)
        {

            this.preemptionAggregationRepository = preemptionAggregationRepository;
        }

        //public override string YAxisTitle => SelectedAggregationType + " of " + Regex.Replace(
        //                                         SelectedAggregatedDataType.DataName,
        //                                         @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1") + " " +
        //                                     TimePeriodOptions.SelectedBinSize + " bins";

        protected override List<BinsContainer> GetBinsContainersBySignal(Location signal, AggregationOptions options)
        {
            var aggregationBySignal = new PreemptionAggregationBySignal(this, signal, preemptionAggregationRepository, options);
            return aggregationBySignal.BinsContainers;
        }

        public override List<BinsContainer> GetBinsContainersByRoute(List<Location> signals, AggregationOptions options)
        {
            var binsContainers = BinFactory.GetBins(options.TimeOptions);
            foreach (var signal in signals)
            {
                var preemptionAggregationBySignal = new PreemptionAggregationBySignal(this, signal, preemptionAggregationRepository, options);
                PopulateBinsForRoute(signals, binsContainers, preemptionAggregationBySignal);
            }
            return binsContainers;
        }

    }
}