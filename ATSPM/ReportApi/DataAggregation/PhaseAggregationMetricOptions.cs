using ATSPM.Application.Business.Aggregation;
using ATSPM.Application.Business.Bins;
using ATSPM.Application.Business.Common;
using ATSPM.Application.Enums;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.TempExtensions;
using ATSPM.Data.Models;
using ATSPM.ReportApi.DataAggregation;
using System.Collections.Concurrent;

namespace MOE.Common.Business.WCFServiceLibrary
{

    public abstract class PhaseAggregationMetricOptions : SignalAggregationMetricOptions
    {
        protected PhaseAggregationMetricOptions(ILocationRepository locationRepository, ILogger<SignalAggregationMetricOptions> logger) : base(locationRepository, logger)
        {
        }

        //public override string YAxisTitle { get; }
        //public List<int> AvailablePhaseNumbers { get; set; }

        protected override List<AggregationResult> GetChartByXAxisAggregation(AggregationOptions options)
        {
            switch (options.SelectedXAxisType)
            {
                case XAxisType.Time:
                    return GetTimeCharts(options);
                case XAxisType.TimeOfDay:
                    return GetTimeOfDayCharts(options);
                case XAxisType.Approach:
                    return GetPhaseCharts(options);
                case XAxisType.Signal:
                    return GetSignalCharts(options);
                default:
                    throw new Exception("Invalid X-Axis");
            }
        }

        protected List<AggregationResult> GetPhaseCharts(AggregationOptions options)
        {
            var charts = new List<AggregationResult>();
            switch (options.SelectedSeries)
            {
                case SeriesType.PhaseNumber:
                    var chart = new AggregationResult();
                    foreach (var signal in options.Signals)
                    {
                        chart.Series.Add(GetPhaseXAxisPhaseSeries(signal, options));
                    }
                    charts.Add(chart);
                    break;
                default:
                    throw new Exception("Invalid X-Axis Series Combination");
            }
            return charts;
        }

        protected override List<AggregationResult> GetTimeCharts(AggregationOptions options)
        {
            var charts = new List<AggregationResult>();
            switch (options.SelectedSeries)
            {
                case SeriesType.PhaseNumber:
                    foreach (var signal in options.Signals)
                    {
                        var availablePhaseNumbers = GetAvailablePhaseNumbers(signal, options);
                        charts.Add(GetTimeXAxisApproachSeriesChart(signal, availablePhaseNumbers, options));
                    }
                    break;
                case SeriesType.Signal:
                    charts.Add(GetTimeXAxisSignalSeriesChart(options.Signals, options));
                    break;
                case SeriesType.Route:
                    charts.Add(GetTimeXAxisRouteSeriesChart(options.Signals, options));
                    break;
                default:
                    throw new Exception("Invalid X-Axis Series Combination");
            }
            return charts;
        }


        protected override List<AggregationResult> GetSignalCharts(AggregationOptions options)
        {
            var charts = new List<AggregationResult>();
            switch (options.SelectedSeries)
            {
                case SeriesType.PhaseNumber:
                    charts.Add(GetSignalsXAxisPhaseNumberSeriesChart(options.Signals, options));
                    break;
                case SeriesType.Signal:
                    var chart = new AggregationResult();
                    chart.Series.Add(GetSignalsXAxisSignalSeries(options.Signals, options));
                    charts.Add(chart);
                    break;
                default:
                    throw new Exception("Invalid X-Axis Series Combination");
            }
            return charts;
        }

        protected AggregationResult GetTimeOfDayXAxisPhaseSeriesChart(Location signal, AggregationOptions options)
        {
            var chart = new AggregationResult();
            var seriesList = new ConcurrentBag<Series>();
            var availablePhaseNumbers = GetAvailablePhaseNumbers(signal, options);
            try
            {
                Parallel.For(0, availablePhaseNumbers.Count, i =>
                {
                    var binsContainers = GetBinsContainersByPhaseNumber(signal, availablePhaseNumbers[i], options);
                    var series = CreateSeries("Phase " + availablePhaseNumbers[i].ToString());
                    SetTimeAggregateSeries(series, binsContainers, options);
                    seriesList.Add(series);
                });
            }
            catch (Exception e)
            {
                throw;
            }
            var orderedSeries = seriesList.OrderBy(s => s.Identifier).ToList();
            foreach (var series in orderedSeries)
                chart.Series.Add(series);
            return chart;
        }


        protected override List<AggregationResult> GetTimeOfDayCharts(AggregationOptions options)
        {
            var charts = new List<AggregationResult>();
            switch (options.SelectedSeries)
            {
                case SeriesType.PhaseNumber:
                    foreach (var signal in options.Signals)
                    {
                        charts.Add(GetTimeOfDayXAxisPhaseSeriesChart(signal, options));
                    }
                    break;
                case SeriesType.Signal:
                    charts.Add(GetTimeOfDayXAxisSignalSeriesChart(options.Signals, options));
                    break;
                case SeriesType.Route:
                    charts.Add(GetTimeOfDayXAxisRouteSeriesChart(options.Signals, options));
                    break;
                default:
                    throw new Exception("Invalid X-Axis Series Combination");
            }
            return charts;
        }

        protected AggregationResult GetSignalsXAxisPhaseNumberSeriesChart(List<Location> signals, AggregationOptions options)
        {
            var chart = new AggregationResult();
            var availablePhaseNumbers = new List<int>();
            foreach (var signal in signals)
                availablePhaseNumbers.AddRange(GetAvailablePhaseNumbers(signal, options));
            availablePhaseNumbers = availablePhaseNumbers.Distinct().ToList();
            var colorCode = 1;
            foreach (var phaseNumber in availablePhaseNumbers)
            {
                var seriesName = "Phase " + phaseNumber;
                Series series = GetSignalXAxisPhaseNumberSeries(signals, phaseNumber, seriesName, options);
                colorCode++;
                chart.Series.Add(series);
            }
            return chart;
        }

        public Series GetSignalXAxisPhaseNumberSeries(List<Location> signals, int phaseNumber, string seriesName, AggregationOptions options)
        {
            var series = CreateSeries(seriesName);
            foreach (var signal in signals)
            {
                var binsContainers = GetBinsContainersByPhaseNumber(signal, phaseNumber, options);
                var dataPoint = new DataPointStringDouble();
                dataPoint.Value = options.SelectedAggregationType == AggregationCalculationType.Sum
                    ? binsContainers.Sum(b => b.SumValue)
                    : binsContainers.Average(b => b.SumValue);
                dataPoint.Identifier = signal.LocationDescription();
                series.DataPoints.Add(dataPoint);
            }

            return series;
        }

        public Series GetPhaseXAxisPhaseSeries(Location signal, AggregationOptions options)
        {
            var series = CreateSeries(signal.LocationDescription());
            var phaseNumbers = signal.GetPhasesForSignal();
            foreach (var phaseNumber in phaseNumbers)
            {
                var dataPoint = new DataPointStringDouble();
                if (options.SelectedAggregationType == AggregationCalculationType.Sum)
                    dataPoint.Value = GetSumByPhaseNumber(signal, phaseNumber, options);
                else
                    dataPoint.Value = GetAverageByPhaseNumber(signal, phaseNumber, options);
                dataPoint.Identifier = "Phase " + phaseNumber;
                series.DataPoints.Add(dataPoint);
            }
            return series;
        }

        protected AggregationResult GetTimeXAxisApproachSeriesChart(Location signal, List<int> availablePhaseNumbers, AggregationOptions options)
        {
            var chart = new AggregationResult();
            foreach (var phase in availablePhaseNumbers)
            {
                chart.Series.Add(GetTimeXAxisPhaseSeries(phase, signal, options));
            }
            return chart;
        }


        private Series GetTimeXAxisPhaseSeries(int phaseNumber, Location signal, AggregationOptions options)
        {
            var binsContainers = GetBinsContainersByPhaseNumber(signal, phaseNumber, options);
            var series = CreateSeries("Phase " + phaseNumber);
            if ((options.TimeOptions.SelectedBinSize == TimeOptions.BinSize.Month ||
                 options.TimeOptions.SelectedBinSize == TimeOptions.BinSize.Year) &&
                options.TimeOptions.TimeOption == TimeOptions.TimePeriodOptions.TimePeriod)
                foreach (var binsContainer in binsContainers)
                {
                    var dataPoint = options.SelectedAggregationType == AggregationCalculationType.Sum
                        ? GetContainerDataPointForSum(binsContainer)
                        : GetContainerDataPointForAverage(binsContainer);
                    series.DataPoints.Add(dataPoint);
                }
            else
                foreach (var bin in binsContainers.FirstOrDefault()?.Bins)
                {
                    var dataPoint = options.SelectedAggregationType == AggregationCalculationType.Sum
                        ? GetDataPointForSum(bin)
                        : GetDataPointForAverage(bin);
                    series.DataPoints.Add(dataPoint);
                }

            return series;
        }

        protected abstract List<int> GetAvailablePhaseNumbers(Location signal, AggregationOptions options);
        protected abstract int GetAverageByPhaseNumber(Location signal, int phaseNumber, AggregationOptions options);
        protected abstract int GetSumByPhaseNumber(Location signal, int phaseNumber, AggregationOptions options);
        protected abstract List<BinsContainer> GetBinsContainersByPhaseNumber(Location signal, int phaseNumber, AggregationOptions options);

    }
}