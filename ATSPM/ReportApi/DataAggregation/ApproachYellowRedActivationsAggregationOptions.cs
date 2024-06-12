#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - MOE.Common.Business.WCFServiceLibrary/ApproachYellowRedActivationsAggregationOptions.cs
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
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using MOE.Common.Business.DataAggregation;
using System.Collections.Concurrent;

namespace MOE.Common.Business.WCFServiceLibrary
{

    public enum YellowRedActivationsDataTypes { SevereRedLightViolations, TotalRedLightViolations, YellowActivations, ViolationTime, Cycles }
    //AggregatedDataTypes = new List<AggregatedDataType>
    //    {
    //        new AggregatedDataType { Id = 0, DataName = "SevereRedLightViolations" },
    //        new AggregatedDataType { Id = 1, DataName = "TotalRedLightViolations" },
    //        new AggregatedDataType { Id = 2, DataName = "YellowActivations" },
    //        new AggregatedDataType { Id = 3, DataName = "ViolationTime" },
    //        new AggregatedDataType { Id = 4, DataName = "Cycles" }
    //    };

    public class ApproachYellowRedActivationsAggregationOptions : ApproachAggregationMetricOptions
    {
        private readonly IApproachYellowRedActivationAggregationRepository approachYellowRedActivationAggregationRepository;

        public ApproachYellowRedActivationsAggregationOptions(
            IApproachYellowRedActivationAggregationRepository approachYellowRedActivationAggregationRepository,
            ILocationRepository locationRepository,
            ILogger<ApproachYellowRedActivationsAggregationOptions> logger) : base(locationRepository, logger)
        {

            this.approachYellowRedActivationAggregationRepository = approachYellowRedActivationAggregationRepository;
        }

        //public override string ChartTitle
        //{
        //    get
        //    {
        //        string chartTitle;
        //        chartTitle = "AggregationChart\n";
        //        chartTitle += TimePeriodOptions.Start.ToString();
        //        if (TimePeriodOptions.End > TimePeriodOptions.Start)
        //            chartTitle += " to " + TimePeriodOptions.End + "\n";
        //        if (TimePeriodOptions.DaysOfWeek != null)
        //            foreach (var dayOfWeek in TimePeriodOptions.DaysOfWeek)
        //                chartTitle += dayOfWeek + " ";
        //        if (TimePeriodOptions.TimeOfDayStartHour != null && TimePeriodOptions.TimeOfDayStartMinute != null &&
        //            TimePeriodOptions.TimeOfDayEndHour != null && TimePeriodOptions.TimeOfDayEndMinute != null)
        //            chartTitle += "Limited to: " +
        //                          new TimeSpan(0, TimePeriodOptions.TimeOfDayStartHour.Value,
        //                              TimePeriodOptions.TimeOfDayStartMinute.Value, 0) + " to " + new TimeSpan(0,
        //                              TimePeriodOptions.TimeOfDayEndHour.Value,
        //                              TimePeriodOptions.TimeOfDayEndMinute.Value, 0) + "\n";
        //        chartTitle += TimePeriodOptions.SelectedBinSize + " bins ";
        //        chartTitle += SelectedXAxisType + " Aggregation ";
        //        chartTitle += SelectedAggregationType.ToString();
        //        return chartTitle;
        //    }
        //}

        //public override string YAxisTitle => SelectedAggregationType + " of " + Regex.Replace(
        //                                         SelectedAggregatedDataType.DataName,
        //                                         @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1") + " " +
        //                                     TimePeriodOptions.SelectedBinSize + " bins";


        protected override int GetAverageByPhaseNumber(Location signal, int phaseNumber, AggregationOptions options)
        {
            var splitFailAggregationBySignal =
                new YellowRedActivationsAggregationBySignal(this, signal, approachYellowRedActivationAggregationRepository, options);
            return splitFailAggregationBySignal.Average;
        }

        protected override double GetSumByPhaseNumber(Location signal, int phaseNumber, AggregationOptions options)
        {
            var splitFailAggregationBySignal =
                new YellowRedActivationsAggregationBySignal(this, signal, approachYellowRedActivationAggregationRepository, options);
            return splitFailAggregationBySignal.Average;
        }

        protected override int GetAverageByDirection(Location signal, DirectionTypes direction, AggregationOptions options)
        {
            var splitFailAggregationBySignal =
                new YellowRedActivationsAggregationBySignal(this, signal, direction, options);
            return splitFailAggregationBySignal.Average;
        }

        protected override double GetSumByDirection(Location signal, DirectionTypes direction, AggregationOptions options)
        {
            var yellowRedActivationsAggregationBySignal =
                new YellowRedActivationsAggregationBySignal(this, signal, direction, options);
            return yellowRedActivationsAggregationBySignal.Average;
        }

        protected override List<BinsContainer> GetBinsContainersBySignal(Location signal, AggregationOptions options)
        {
            var yellowRedActivationsAggregationBySignal = new YellowRedActivationsAggregationBySignal(this, signal, approachYellowRedActivationAggregationRepository, options);
            return yellowRedActivationsAggregationBySignal.BinsContainers;
        }

        protected override List<BinsContainer> GetBinsContainersByDirection(DirectionTypes directionType,
            Location signal, AggregationOptions options)
        {
            var splitFailAggregationBySignal =
                new YellowRedActivationsAggregationBySignal(this, signal, directionType, options);
            return splitFailAggregationBySignal.BinsContainers;
        }

        protected override List<BinsContainer> GetBinsContainersByPhaseNumber(Location signal, int phaseNumber, AggregationOptions options)
        {
            var splitFailAggregationBySignal =
                new YellowRedActivationsAggregationBySignal(this, signal, phaseNumber, approachYellowRedActivationAggregationRepository, options);
            return splitFailAggregationBySignal.BinsContainers;
        }

        public override List<BinsContainer> GetBinsContainersByRoute(List<Location> signals, AggregationOptions options)
        {
            var aggregations = new ConcurrentBag<YellowRedActivationsAggregationBySignal>();
            Parallel.ForEach(signals,
                signal => { aggregations.Add(new YellowRedActivationsAggregationBySignal(this, signal, approachYellowRedActivationAggregationRepository, options)); });
            var binsContainers = BinFactory.GetBins(options.TimeOptions);
            foreach (var splitFailAggregationBySignal in aggregations)
                for (var i = 0; i < binsContainers.Count; i++)
                    for (var binIndex = 0; binIndex < binsContainers[i].Bins.Count; binIndex++)
                    {
                        var bin = binsContainers[i].Bins[binIndex];
                        bin.Sum += splitFailAggregationBySignal.BinsContainers[i].Bins[binIndex].Sum;
                        bin.Average = Convert.ToInt32(Math.Round((double)(bin.Sum / signals.Count)));
                    }
            return binsContainers;
        }


        protected override List<BinsContainer> GetBinsContainersByApproach(Approach approach, bool getprotectedPhase, AggregationOptions options)
        {
            var approachYellowRedActivationsAggregationContainer = new YellowRedActivationsAggregationByApproach(
                approach,
                this,
                options.Start,
                options.End,
                getprotectedPhase,
                options.DataType,
                approachYellowRedActivationAggregationRepository,
                options
                );
            return approachYellowRedActivationsAggregationContainer.BinsContainers;
        }
    }
}