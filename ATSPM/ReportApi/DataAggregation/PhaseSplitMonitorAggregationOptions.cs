using ATSPM.Application.Business.Aggregation;
using ATSPM.Application.Business.Bins;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data.Models;
using MOE.Common.Business.DataAggregation;
using System.Collections.Concurrent;

namespace MOE.Common.Business.WCFServiceLibrary
{
    public enum SplitMonitorDataTypes
    {
        EightyFifthPercentileSplit,
        SkippedCount
    }
    //            AggregatedDataTypes = new List<AggregatedDataType>
    //            {
    //                new AggregatedDataType { Id = 0, DataName = PhaseSplitMonitorAggregationByPhase.EIGHTY_FIFTH_PERCENTILE_SPLIT
    //},
    //                new AggregatedDataType { Id = 1, DataName = PhaseSplitMonitorAggregationByPhase.SKIPPED_COUNT }
    //            };

    public class PhaseSplitMonitorAggregationOptions : PhaseAggregationMetricOptions
    {
        protected readonly IPhaseSplitMonitorAggregationRepository phaseSplitMonitorAggregationRepository;

        public PhaseSplitMonitorAggregationOptions(
            IPhaseSplitMonitorAggregationRepository phaseSplitMonitorAggregationRepository,
            ILocationRepository locationRepository,
            ILogger<PhaseSplitMonitorAggregationOptions> logger) : base(locationRepository, logger)
        {
            this.phaseSplitMonitorAggregationRepository = phaseSplitMonitorAggregationRepository;
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


        protected override List<int> GetAvailablePhaseNumbers(Location signal, AggregationOptions options)
        {
            return phaseSplitMonitorAggregationRepository.GetAggregationsBetweenDates(signal.LocationIdentifier, options.Start, options.End).Select(a => a.PhaseNumber).Distinct().ToList();
        }

        protected override int GetAverageByPhaseNumber(Location signal, int phaseNumber, AggregationOptions options)
        {
            var splitFailAggregationBySignal =
                new PhaseSplitMonitorAggregationBySignal(this, signal, phaseSplitMonitorAggregationRepository, options);
            return splitFailAggregationBySignal.Average;
        }

        protected override int GetSumByPhaseNumber(Location signal, int phaseNumber, AggregationOptions options)
        {
            var splitFailAggregationBySignal =
                new PhaseSplitMonitorAggregationBySignal(this, signal, phaseSplitMonitorAggregationRepository, options);
            return splitFailAggregationBySignal.Average;
        }

        protected override List<BinsContainer> GetBinsContainersBySignal(Location signal, AggregationOptions options)
        {
            var phaseTerminationAggregationBySignal = new PhaseSplitMonitorAggregationBySignal(this, signal, phaseSplitMonitorAggregationRepository, options);
            return phaseTerminationAggregationBySignal.BinsContainers;
        }


        protected override List<BinsContainer> GetBinsContainersByPhaseNumber(Location signal, int phaseNumber, AggregationOptions options)
        {
            var phaseTerminationAggregationBySignal =
                new PhaseSplitMonitorAggregationBySignal(this, signal, phaseNumber, phaseSplitMonitorAggregationRepository, options);
            return phaseTerminationAggregationBySignal.BinsContainers;
        }

        public override List<BinsContainer> GetBinsContainersByRoute(List<Location> signals, AggregationOptions options)
        {
            var aggregations = new ConcurrentBag<PhaseSplitMonitorAggregationBySignal>();
            Parallel.ForEach(signals, signal => { aggregations.Add(new PhaseSplitMonitorAggregationBySignal(this, signal, phaseSplitMonitorAggregationRepository, options)); });
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



    }
}