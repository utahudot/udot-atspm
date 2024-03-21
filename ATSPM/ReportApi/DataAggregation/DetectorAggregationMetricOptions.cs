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

    public abstract class DetectorAggregationMetricOptions : ApproachAggregationMetricOptions
    {
        protected DetectorAggregationMetricOptions(ILocationRepository locationRepository, ILogger<ApproachAggregationMetricOptions> logger) : base(locationRepository, logger)
        {
        }

        //public override string YAxisTitle { get; }

        public override List<AggregationResult> CreateMetric(AggregationOptions options)
        {
            GetSignalObjects(options);
            if (options.SelectedXAxisType == XAxisType.TimeOfDay &&
                options.TimeOptions.TimeOption == TimeOptions.TimePeriodOptions.StartToEnd)
            {
                options.TimeOptions.TimeOption = TimeOptions.TimePeriodOptions.TimePeriod;
                options.TimeOptions.TimeOfDayStartHour = 0;
                options.TimeOptions.TimeOfDayStartMinute = 0;
                options.TimeOptions.TimeOfDayEndHour = 23;
                options.TimeOptions.TimeOfDayEndMinute = 59;
                if (options.TimeOptions.DaysOfWeek == null)
                    options.TimeOptions.DaysOfWeek = new List<DayOfWeek>
                    {
                        DayOfWeek.Sunday,
                        DayOfWeek.Monday,
                        DayOfWeek.Tuesday,
                        DayOfWeek.Wednesday,
                        DayOfWeek.Thursday,
                        DayOfWeek.Friday,
                        DayOfWeek.Saturday
                    };
            }
            return base.CreateMetric(options);
        }


        protected override List<AggregationResult> GetChartByXAxisAggregation(AggregationOptions options)
        {
            switch (options.SelectedXAxisType)
            {
                case XAxisType.Time:
                    return GetTimeCharts(options);
                case XAxisType.TimeOfDay:
                    return GetTimeOfDayCharts(options);
                case XAxisType.Approach:
                    return GetApproachCharts(options);
                case XAxisType.Direction:
                    return GetDirectionCharts(options);
                case XAxisType.Signal:
                    return GetSignalCharts(options);
                case XAxisType.Detector:
                    return GetDetectorCharts(options);
                default:
                    throw new Exception("Invalid X-Axis");
            }
        }

        private List<AggregationResult> GetDetectorCharts(AggregationOptions options)
        {
            var charts = new List<AggregationResult>();
            switch (options.SelectedSeries)
            {
                case SeriesType.PhaseNumber:
                    foreach (var signal in options.Signals)
                    {
                        charts.Add(GetApproachXAxisChart(signal, options));
                    }
                    break;
                case SeriesType.Detector:
                    foreach (var signal in options.Signals)
                    {
                        charts.Add(GetApproachXAxisDetectorSeriesChart(signal, options));
                    }
                    break;
                default:
                    throw new Exception("Invalid X-Axis Series Combination");
            }
            return charts;
        }

        private AggregationResult GetApproachXAxisDetectorSeriesChart(Location signal, AggregationOptions options)
        {
            var chart = new AggregationResult();
            var series = CreateSeries(signal.LocationDescription());
            foreach (var approach in signal.Approaches)
                foreach (var detector in approach.Detectors)
                {
                    var binsContainers = GetBinsContainersByDetector(detector);
                    var dataPoint = new DataPointStringDouble();
                    if (options.SelectedAggregationType == AggregationCalculationType.Sum)
                        dataPoint.Value = binsContainers.FirstOrDefault().SumValue;
                    else
                        dataPoint.Value = binsContainers.FirstOrDefault().AverageValue;
                    dataPoint.Identifier = detector.DectectorIdentifier;
                    series.DataPoints.Add(dataPoint);
                }
            chart.Series.Add(series);
            return chart;
        }


        protected override List<AggregationResult> GetTimeCharts(AggregationOptions options)
        {
            List<AggregationResult> charts = new List<AggregationResult>();
            switch (options.SelectedSeries)
            {
                case SeriesType.PhaseNumber:
                    foreach (var signal in options.Signals)
                    {
                        charts.Add(GetTimeXAxisApproachSeriesChart(signal, options));
                    }
                    break;
                case SeriesType.Direction:
                    foreach (var signal in options.Signals)
                    {
                        charts.Add(GetTimeXAxisDirectionSeriesChart(signal, options));
                    }
                    break;
                case SeriesType.Detector:
                    foreach (var signal in options.Signals)
                    {
                        charts.Add(GetTimeXAxisDetectorSeriesChart(signal, options));
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

        private AggregationResult GetTimeXAxisDetectorSeriesChart(Location signal, AggregationOptions options)
        {
            var chart = new AggregationResult();
            foreach (var approach in signal.Approaches)
                foreach (var detector in approach.Detectors)
                {
                    var binsContainers = GetBinsContainersByDetector(detector);
                    var series = CreateSeries(detector.DectectorIdentifier);
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
                    chart.Series.Add(series);
                }
            return chart;
        }

        protected override List<AggregationResult> GetSignalCharts(AggregationOptions options)
        {
            List<AggregationResult> charts = new List<AggregationResult>();
            AggregationResult chart = new AggregationResult();
            charts.Add(chart);
            switch (options.SelectedSeries)
            {
                case SeriesType.PhaseNumber:
                    chart.Series.AddRange(GetSignalsXAxisPhaseNumberSeries(options.Signals, options));
                    break;
                case SeriesType.Direction:
                    chart.Series.AddRange(GetSignalsXAxisDirectionSeries(options.Signals, options));
                    break;
                case SeriesType.Signal:
                    chart.Series.Add(GetSignalsXAxisSignalSeries(options.Signals, options));
                    break;
                default:
                    throw new Exception("Invalid X-Axis Series Combination");
            }
            return charts;
        }


        protected override List<AggregationResult> GetTimeOfDayCharts(AggregationOptions options)
        {
            List<AggregationResult> charts = new List<AggregationResult>();
            switch (options.SelectedSeries)
            {
                case SeriesType.PhaseNumber:
                    foreach (var signal in options.Signals)
                    {
                        charts.Add(GetTimeOfDayXAxisApproachSeriesChart(signal, options));
                    }
                    break;
                case SeriesType.Direction:
                    foreach (var signal in options.Signals)
                    {
                        charts.Add(GetTimeOfDayXAxisDirectionSeriesChart(signal, options));
                    }
                    break;
                case SeriesType.Signal:
                    charts.Add(GetTimeOfDayXAxisSignalSeriesChart(options.Signals, options));
                    break;
                case SeriesType.Route:
                    charts.Add(GetTimeOfDayXAxisRouteSeriesChart(options.Signals, options));
                    break;
                case SeriesType.Detector:
                    foreach (var signal in options.Signals)
                    {
                        charts.Add(GetTimeOfDayXAxisDetectorSeriesChart(signal, options));
                    }
                    break;
                default:
                    throw new Exception("Invalid X-Axis Series Combination");
            }
            return charts;
        }

        private AggregationResult GetTimeOfDayXAxisDetectorSeriesChart(Location signal, AggregationOptions options)
        {
            var chart = new AggregationResult();
            var seriesList = new ConcurrentBag<Series>();
            foreach (var approach in signal.Approaches)
            {
                var detectors = approach.Detectors.ToList();
                Parallel.For(0, detectors.Count, i =>
                {
                    var binsContainers = GetBinsContainersByDetector(detectors[i]);
                    var series = CreateSeries(detectors[i].DectectorIdentifier);
                    SetTimeAggregateSeries(series, binsContainers, options);
                    seriesList.Add(series);
                    ;
                });
            }
            var orderedSeries = seriesList.OrderBy(s => s.Identifier).ToList();
            foreach (var series in orderedSeries)
            {
                chart.Series.Add(series);
            }
            return chart;
        }


        protected abstract List<BinsContainer> GetBinsContainersByDetector(Detector detector);
    }
}