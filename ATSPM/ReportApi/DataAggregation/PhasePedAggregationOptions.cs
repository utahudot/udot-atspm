using ATSPM.Application.Business.Bins;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data.Models;
using MOE.Common.Business.DataAggregation;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace MOE.Common.Business.WCFServiceLibrary
{

    public class PhasePedAggregationOptions : PhaseAggregationMetricOptions
    {
        private readonly IPhasePedAggregationRepository phasePedAggregationRepository;

        public PhasePedAggregationOptions(
            IPhasePedAggregationRepository phasePedAggregationRepository,
            ILocationRepository locationRepository,
            ILogger<PhasePedAggregationOptions> logger) : base(locationRepository, logger)
        {
            AggregatedDataTypes = new List<AggregatedDataType>
            {
                new AggregatedDataType { Id = 0, DataName = PhasePedAggregationByPhase.PED_CYCLES },
                new AggregatedDataType { Id = 1, DataName = PhasePedAggregationByPhase.PED_DELAY_SUM },
                new AggregatedDataType { Id = 2, DataName = PhasePedAggregationByPhase.MIN_PED_DELAY },
                new AggregatedDataType { Id = 3, DataName = PhasePedAggregationByPhase.MAX_PED_DELAY },
                new AggregatedDataType { Id = 4, DataName = PhasePedAggregationByPhase.PED_REQUESTS }
            };
            this.phasePedAggregationRepository = phasePedAggregationRepository;
        }

        public override string ChartTitle
        {
            get
            {
                string chartTitle;
                chartTitle = "AggregationChart\n";
                chartTitle += TimeOptions.Start.ToString();
                if (TimeOptions.End > TimeOptions.Start)
                    chartTitle += " to " + TimeOptions.End + "\n";
                if (TimeOptions.DaysOfWeek != null)
                    foreach (var dayOfWeek in TimeOptions.DaysOfWeek)
                        chartTitle += dayOfWeek + " ";
                if (TimeOptions.TimeOfDayStartHour != null && TimeOptions.TimeOfDayStartMinute != null &&
                    TimeOptions.TimeOfDayEndHour != null && TimeOptions.TimeOfDayEndMinute != null)
                    chartTitle += "Limited to: " +
                                  new TimeSpan(0, TimeOptions.TimeOfDayStartHour.Value,
                                      TimeOptions.TimeOfDayStartMinute.Value, 0) + " to " + new TimeSpan(0,
                                      TimeOptions.TimeOfDayEndHour.Value,
                                      TimeOptions.TimeOfDayEndMinute.Value, 0) + "\n";
                chartTitle += TimeOptions.SelectedBinSize + " bins ";
                chartTitle += SelectedXAxisType + " Aggregation ";
                chartTitle += SelectedAggregationType.ToString();
                return chartTitle;
            }
        }

        public override string YAxisTitle => SelectedAggregationType + " of " + Regex.Replace(
                                                 SelectedAggregatedDataType.DataName,
                                                 @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1") + " " +
                                             TimeOptions.SelectedBinSize + " bins";


        protected override List<int> GetAvailablePhaseNumbers(Location signal)
        {
            return phasePedAggregationRepository.GetAggregationsBetweenDates(signal.LocationIdentifier, this.Start, this.End).Select(a => a.PhaseNumber).Distinct().ToList();
        }

        protected override int GetAverageByPhaseNumber(Location signal, int phaseNumber)
        {
            var splitFailAggregationBySignal =
                new PhasePedAggregationBySignal(this, signal, phasePedAggregationRepository);
            return splitFailAggregationBySignal.Average;
        }

        protected override int GetSumByPhaseNumber(Location signal, int phaseNumber)
        {
            var splitFailAggregationBySignal =
                new PhasePedAggregationBySignal(this, signal, phasePedAggregationRepository);
            return splitFailAggregationBySignal.Average;
        }

        protected override List<BinsContainer> GetBinsContainersBySignal(Location signal)
        {
            var phaseTerminationAggregationBySignal = new PhasePedAggregationBySignal(this, signal, phasePedAggregationRepository);
            return phaseTerminationAggregationBySignal.BinsContainers;
        }


        protected override List<BinsContainer> GetBinsContainersByPhaseNumber(Location signal, int phaseNumber)
        {
            var phaseTerminationAggregationBySignal =
                new PhasePedAggregationBySignal(this, signal, phaseNumber, phasePedAggregationRepository);
            return phaseTerminationAggregationBySignal.BinsContainers;
        }

        public override List<BinsContainer> GetBinsContainersByRoute(List<Location> signals)
        {
            var aggregations = new ConcurrentBag<PhasePedAggregationBySignal>();
            Parallel.ForEach(signals, signal => { aggregations.Add(new PhasePedAggregationBySignal(this, signal, phasePedAggregationRepository)); });
            var binsContainers = BinFactory.GetBins(TimeOptions);
            foreach (var phasePedAggregationBySignal in aggregations)
                for (var i = 0; i < binsContainers.Count; i++)
                    for (var binIndex = 0; binIndex < binsContainers[i].Bins.Count; binIndex++)
                    {
                        var bin = binsContainers[i].Bins[binIndex];
                        bin.Sum += phasePedAggregationBySignal.BinsContainers[i].Bins[binIndex].Sum;
                        bin.Average = Convert.ToInt32(Math.Round((double)(bin.Sum / signals.Count)));
                    }
            return binsContainers;
        }



    }
}