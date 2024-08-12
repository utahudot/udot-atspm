﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - ATSPM.ReportApi.DataAggregation/PhaseLeftTurnGapAggregationOptions.cs
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

using System.Collections.Concurrent;
using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.ReportApi.DataAggregation
{
    public enum LeftTurnGapDataTypes
    {
        GapCount1,
        GapCount2,
        GapCount3,
        GapCount4,
        GapCount5,
        GapCount6,
        GapCount7,
        GapCount8,
        GapCount9,
        GapCount10,
        GapCount11,
        SumGapDuration1,
        SumGapDuration2,
        SumGapDuration3,
        SumGreenTime
    }

    //public const string GAP_COUNT_1 = "GapCount1";
    //public const string GAP_COUNT_2 = "GapCount2";
    //public const string GAP_COUNT_3 = "GapCount3";
    //public const string GAP_COUNT_4 = "GapCount4";
    //public const string GAP_COUNT_5 = "GapCount5";
    //public const string GAP_COUNT_6 = "GapCount6";
    //public const string GAP_COUNT_7 = "GapCount7";
    //public const string GAP_COUNT_8 = "GapCount8";
    //public const string GAP_COUNT_9 = "GapCount9";
    //public const string GAP_COUNT_10 = "GapCount10";
    //public const string GAP_COUNT_11 = "GapCount11";
    //public const string SUM_GAP_DURATION_1 = "SumGapDuration1";
    //public const string SUM_GAP_DURATION_2 = "SumGapDuration2";
    //public const string SUM_GAP_DURATION_3 = "SumGapDuration3";
    //public const string SUM_GREEN_TIME = "SumGreenTime";
    //    AggregatedDataTypes = new List<AggregatedDataType>
    //            {
    //                new AggregatedDataType { Id = 0, DataName = GAP_COUNT_1
    //},
    //                new AggregatedDataType { Id = 1, DataName = GAP_COUNT_2 },
    //                new AggregatedDataType { Id = 2, DataName = GAP_COUNT_3 },
    //                new AggregatedDataType { Id = 3, DataName = GAP_COUNT_4 },
    //                new AggregatedDataType { Id = 4, DataName = GAP_COUNT_5 },
    //                new AggregatedDataType { Id = 5, DataName = GAP_COUNT_6 },
    //                new AggregatedDataType { Id = 6, DataName = GAP_COUNT_7 },
    //                new AggregatedDataType { Id = 7, DataName = GAP_COUNT_8 },
    //                new AggregatedDataType { Id = 8, DataName = GAP_COUNT_9 },
    //                new AggregatedDataType { Id = 9, DataName = GAP_COUNT_10 },
    //                new AggregatedDataType { Id = 10, DataName = GAP_COUNT_11 },
    //                new AggregatedDataType { Id = 11, DataName = SUM_GAP_DURATION_1 },
    //                new AggregatedDataType { Id = 12, DataName = SUM_GAP_DURATION_2 },
    //                new AggregatedDataType { Id = 13, DataName = SUM_GAP_DURATION_3 },
    //                new AggregatedDataType { Id = 14, DataName = SUM_GREEN_TIME }
    //            };

    public class PhaseLeftTurnGapAggregationOptions : ApproachAggregationMetricOptions
    {

        private readonly IPhaseLeftTurnGapAggregationRepository phaseLeftTurnGapAggregationRepository;

        public PhaseLeftTurnGapAggregationOptions(
            IPhaseLeftTurnGapAggregationRepository phaseLeftTurnGapAggregationRepository,
            ILocationRepository locationRepository,
            ILogger<PhaseLeftTurnGapAggregationOptions> logger) : base(locationRepository, logger)
        {

            this.phaseLeftTurnGapAggregationRepository = phaseLeftTurnGapAggregationRepository;
        }

        //public override string ChartTitle
        //{
        //    get
        //    {
        //        string chartTitle;
        //        chartTitle = "AggregationChart\n";
        //        chartTitle += TimeOptions.Start.ToString();
        //        if (TimeOptions.End > TimeOptions.Start)
        //            chartTitle += " to " + TimeOptions.End + "\n";
        //        if (TimeOptions.DaysOfWeek != null)
        //            foreach (var dayOfWeek in TimeOptions.DaysOfWeek)
        //                chartTitle += dayOfWeek + " ";
        //        if (TimeOptions.TimeOfDayStartHour != null && TimeOptions.TimeOfDayStartMinute != null &&
        //            TimeOptions.TimeOfDayEndHour != null && TimeOptions.TimeOfDayEndMinute != null)
        //            chartTitle += " Limited to: " +
        //                          new TimeSpan(0, TimeOptions.TimeOfDayStartHour.Value,
        //                              TimeOptions.TimeOfDayStartMinute.Value, 0) + " to " + new TimeSpan(0,
        //                              TimeOptions.TimeOfDayEndHour.Value,
        //                              TimeOptions.TimeOfDayEndMinute.Value, 0) + "\n";
        //        chartTitle += TimeOptions.SelectedBinSize + " bins ";
        //        chartTitle += SelectedXAxisType + " Aggregation ";
        //        chartTitle += SelectedAggregationType.ToString();
        //        return chartTitle;
        //    }
        //}

        //public override string YAxisTitle => Regex.Replace(SelectedAggregationType + " of " +
        //                                         SelectedAggregatedDataType.DataName.ToString(),
        //                                         @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1") + " " +
        //                                     TimeOptions.SelectedBinSize + " bins";


        protected override int GetAverageByPhaseNumber(Location signal, int phaseNumber, AggregationOptions options)
        {
            var phaseCycleAggregationBySignal =
                new PhaseLeftTurnGapAggregationBySignal(this, signal, phaseLeftTurnGapAggregationRepository, options);
            return phaseCycleAggregationBySignal.Average;
        }

        protected override double GetSumByPhaseNumber(Location signal, int phaseNumber, AggregationOptions options)
        {
            var phaseCycleAggregationBySignal =
                new PhaseLeftTurnGapAggregationBySignal(this, signal, phaseLeftTurnGapAggregationRepository, options);
            return phaseCycleAggregationBySignal.Average;
        }

        protected override int GetAverageByDirection(Location signal, DirectionTypes direction, AggregationOptions options)
        {
            var phaseCycleAggregationBySignal =
                new PhaseLeftTurnGapAggregationBySignal(this, signal, direction, options);
            return phaseCycleAggregationBySignal.Average;
        }

        protected override double GetSumByDirection(Location signal, DirectionTypes direction, AggregationOptions options)
        {
            var phaseCycleAggregationBySignal =
                new PhaseLeftTurnGapAggregationBySignal(this, signal, direction, options);
            return phaseCycleAggregationBySignal.Average;
        }

        protected override List<BinsContainer> GetBinsContainersBySignal(Location signal, AggregationOptions options)
        {
            var phaseCycleAggregationBySignal = new PhaseLeftTurnGapAggregationBySignal(this, signal, phaseLeftTurnGapAggregationRepository, options);
            return phaseCycleAggregationBySignal.BinsContainers;
        }

        protected override List<BinsContainer> GetBinsContainersByDirection(DirectionTypes directionType,
            Location signal, AggregationOptions options)
        {
            var phaseCycleAggregationBySignal =
                new PhaseLeftTurnGapAggregationBySignal(this, signal, directionType, options);
            return phaseCycleAggregationBySignal.BinsContainers;
        }

        protected override List<BinsContainer> GetBinsContainersByPhaseNumber(Location signal, int phaseNumber, AggregationOptions options)
        {
            var splitFailAggregationBySignal =
                new PhaseLeftTurnGapAggregationBySignal(this, signal, phaseNumber, options);
            return splitFailAggregationBySignal.BinsContainers;
        }

        public override List<BinsContainer> GetBinsContainersByRoute(List<Location> signals, AggregationOptions options)
        {
            var aggregations = new ConcurrentBag<PhaseLeftTurnGapAggregationBySignal>();
            Parallel.ForEach(signals, signal => { aggregations.Add(new PhaseLeftTurnGapAggregationBySignal(this, signal, phaseLeftTurnGapAggregationRepository, options)); });
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
            var approachCycleAggregationContainer = new PhaseLeftTurnGapAggregationByApproach(
                approach,
                this,
                options.Start,
                options.End,
                getprotectedPhase,
                options.DataType,
                phaseLeftTurnGapAggregationRepository,
                options
                );
            return approachCycleAggregationContainer.BinsContainers;
        }
    }
}