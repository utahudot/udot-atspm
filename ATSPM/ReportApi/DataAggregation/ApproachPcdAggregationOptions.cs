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
    public enum PcdAggregationDataTypes { ArrivalsOnGreen, ArrivalsOnRed, ArrivalsOnYellow, Volume, TotalDelay }

    //AggregatedDataTypes = new List<AggregatedDataType>
    //    {
    //        new AggregatedDataType { Id = 0, DataName = PcdAggregationByApproach.ARRIVALS_ON_GREEN},
    //        new AggregatedDataType { Id = 1, DataName = PcdAggregationByApproach.ARRIVALS_ON_RED },
    //        new AggregatedDataType { Id = 2, DataName = PcdAggregationByApproach.ARRIVALS_ON_YELLOW },
    //        new AggregatedDataType { Id = 3, DataName = PcdAggregationByApproach.VOLUME },
    //        new AggregatedDataType { Id = 4, DataName = PcdAggregationByApproach.TOTAL_DELAY }
    //    };

    public class ApproachPcdAggregationOptions : ApproachAggregationMetricOptions
    {
        private readonly IApproachPcdAggregationRepository approachPcdAggregationRepository;

        public ApproachPcdAggregationOptions(
            IApproachPcdAggregationRepository approachPcdAggregationRepository,
            ILocationRepository locationRepository,
            ILogger<ApproachPcdAggregationOptions> logger) : base(locationRepository, logger)
        {

            this.approachPcdAggregationRepository = approachPcdAggregationRepository;
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
        //            chartTitle += "Limited to: " +
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

        //public override string YAxisTitle => SelectedAggregationType + " of " + Regex.Replace(
        //                                         SelectedAggregatedDataType.DataName,
        //                                         @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1") + " " +
        //                                     TimeOptions.SelectedBinSize + " bins";


        protected override int GetAverageByPhaseNumber(Location signal, int phaseNumber, AggregationOptions options)
        {
            var splitFailAggregationBySignal =
                new PcdAggregationBySignal(this, signal, options);
            return splitFailAggregationBySignal.Average;
        }

        protected override double GetSumByPhaseNumber(Location signal, int phaseNumber, AggregationOptions options)
        {
            var splitFailAggregationBySignal =
                new PcdAggregationBySignal(this, signal, options);
            return splitFailAggregationBySignal.Average;
        }

        protected override int GetAverageByDirection(Location signal, DirectionTypes direction, AggregationOptions options)
        {
            var splitFailAggregationBySignal =
                new PcdAggregationBySignal(this, signal, direction, options);
            return splitFailAggregationBySignal.Average;
        }


        protected override double GetSumByDirection(Location signal, DirectionTypes direction, AggregationOptions options)
        {
            var splitFailAggregationBySignal =
                new PcdAggregationBySignal(this, signal, direction, options);
            return splitFailAggregationBySignal.Average;
        }

        protected override List<BinsContainer> GetBinsContainersBySignal(Location signal, AggregationOptions options)
        {
            var splitFailAggregationBySignal = new PcdAggregationBySignal(this, signal, options);
            return splitFailAggregationBySignal.BinsContainers;
        }

        protected override List<BinsContainer> GetBinsContainersByDirection(DirectionTypes directionType, Location signal, AggregationOptions options)
        {
            var splitFailAggregationBySignal =
                new PcdAggregationBySignal(this, signal, directionType, options);
            return splitFailAggregationBySignal.BinsContainers;
        }

        protected override List<BinsContainer> GetBinsContainersByPhaseNumber(Location signal, int phaseNumber, AggregationOptions options)
        {
            var splitFailAggregationBySignal =
                new PcdAggregationBySignal(this, signal, phaseNumber, approachPcdAggregationRepository, options);
            return splitFailAggregationBySignal.BinsContainers;
        }

        public override List<BinsContainer> GetBinsContainersByRoute(List<Location> signals, AggregationOptions options)
        {
            var aggregations = new ConcurrentBag<PcdAggregationBySignal>();
            Parallel.ForEach(signals, signal => { aggregations.Add(new PcdAggregationBySignal(this, signal, options)); });
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
            var approachPcdAggregationContainer = new PcdAggregationByApproach(
                approach,
                this,
                options.Start,
                options.End,
                getprotectedPhase,
                options.DataType,
                approachPcdAggregationRepository,
                options
                );
            return approachPcdAggregationContainer.BinsContainers;
        }


    }
}