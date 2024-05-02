using ATSPM.Application.Business.Aggregation;
using ATSPM.Application.Business.Aggregation.FilterExtensions;
using ATSPM.Application.Business.Bins;
using ATSPM.Application.Business.Common;
using ATSPM.Application.Enums;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.TempExtensions;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.ReportApi.DataAggregation;
using Microsoft.EntityFrameworkCore;
using MOE.Common.Business.DataAggregation;
using System.Collections.Concurrent;

namespace MOE.Common.Business.WCFServiceLibrary
{


    //public class AggregatedDataType
    //{

    //    public int Id { get; set; }


    //    public string DataName { get; set; }
    //}


    public abstract class SignalAggregationMetricOptions : ISignalAggregationMetricOptions
    {
        //public List<AggregatedDataType> AggregatedDataTypes;
        private readonly ILocationRepository locationRepository;
        private readonly ILogger logger;

        protected SignalAggregationMetricOptions(ILocationRepository locationRepository, ILogger logger)
        {
            this.locationRepository = locationRepository;
            this.logger = logger;
        }





        //public void CopySignalAggregationBaseValues(SignalAggregationMetricOptions options)
        //{
        //    this.TimeOptions = options.TimeOptions;
        //    this.FilterDirections = options.FilterDirections;
        //    this.FilterMovements = options.FilterMovements;
        //    this.FilterSignals = options.FilterSignals;
        //    this.Start = options.End;
        //    this.Start = options.Start;
        //    this.SelectedAggregatedDataType = AggregatedDataTypes[0];
        //    this.SelectedAggregationType = options.SelectedAggregationType;
        //    this.SelectedDimension = options.SelectedDimension;
        //    this.SelectedSeries = options.SelectedSeries;
        //    this.SelectedXAxisType = options.SelectedXAxisType;
        //    this.SeriesWidth = options.SeriesWidth;
        //    this.ShowEventCount = options.ShowEventCount;
        //}

        public virtual List<AggregationResult> CreateMetric(AggregationOptions options)
        {
            //Y2AxisTitle = "Event Count";
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
            return GetChartByXAxisAggregation(options);
        }

        protected virtual List<AggregationResult> GetChartByXAxisAggregation(AggregationOptions options)
        {
            var signals = GetSignalObjects(options);
            switch (options.SelectedXAxisType)
            {
                case XAxisType.Time:
                    return GetTimeCharts(options, signals);
                case XAxisType.TimeOfDay:
                    return GetTimeOfDayCharts(options, signals);
                case XAxisType.Signal:
                    return GetSignalCharts(options, signals);
                default:
                    throw new Exception("Invalid X-Axis");
            }
        }


        public List<DirectionTypes> GetFilteredDirections(AggregationOptions options)
        {
            Type enumType = typeof(DirectionTypes);
            var enumValues = ((DirectionTypes[])Enum.GetValues(enumType)).ToList();
            var includedDirections = options.FilterDirections.Where(f => f.Include).Select(f => f.DirectionTypeId).ToList();
            var directionsList = enumValues.Where(d => includedDirections.Contains((int)d)).ToList();
            return directionsList;
        }


        protected List<Location> GetSignalObjects(AggregationOptions options)
        {
            var locations = new List<Location>();
            try
            {
                if (options.LocationIdentifiers.Any())
                {
                    foreach (var filterSignal in options.Locations)
                        if (!filterSignal.Exclude)
                        {
                            var signals =
                                locationRepository.GetLocationsBetweenDates(filterSignal.LocationIdentifier, options.TimeOptions.Start, options.TimeOptions.End).ToList();
                            foreach (var signal in signals)
                            {
                                RemoveApproachesByFilter(filterSignal, signal, options);
                                signal.Approaches = signal.Approaches.OrderBy(a => a.ProtectedPhaseNumber).ToList();
                            }
                            locations.AddRange(signals);
                        }
                }
                return locations;
            }
            catch (Exception e)
            {
                logger.LogError($"Unable to apply signal filter {e.Message}");
                throw new Exception("Unable to apply signal filter");
            }
        }


        protected virtual List<AggregationResult> GetTimeCharts(AggregationOptions options, List<Location> signals)
        {
            AggregationResult chart;
            switch (options.SelectedSeries)
            {
                case SeriesType.Signal:
                    //chart = ChartFactory.CreateTimeXIntYChart(this, Signals);
                    return new List<AggregationResult> { GetTimeXAxisSignalSeriesChart(signals, options) };
                case SeriesType.Route:
                    //chart = ChartFactory.CreateTimeXIntYChart(this, Signals);
                    return new List<AggregationResult> { GetTimeXAxisRouteSeriesChart(signals, options) };
                default:
                    throw new Exception("Invalid X-Axis Series Combination");
            }
            //if (ShowEventCount)
            //{
            //    SetTimeXAxisRouteSeriesForEventCount(Signals, chart);
            //}
            //SaveChartImage(chart);
        }

        protected virtual List<AggregationResult> GetSignalCharts(AggregationOptions options, List<Location> signals)
        {
            List<AggregationResult> charts = new List<AggregationResult>();
            var chart = new AggregationResult();
            charts.Add(chart);
            switch (options.SelectedSeries)
            {
                case SeriesType.Signal:
                    //chart = ChartFactory.CreateStringXIntYChart(this);
                    chart.Series.Add(GetSignalsXAxisSignalSeries(signals, options));
                    break;
                default:
                    throw new Exception("Invalid X-Axis Series Combination");
            }
            return charts;
        }

        protected Series GetSignalsXAxisSignalSeries(List<Location> signals, AggregationOptions options)
        {
            var seriesName = "Signals";
            Series series = GetSignalsXAxisSignalSeries(signals, seriesName, options);
            return series;
        }

        public Series GetSignalsXAxisSignalSeries(List<Location> signals, string seriesName, AggregationOptions options)
        {
            var series = new Series { Identifier = seriesName };
            foreach (var signal in signals)
            {
                var binsContainers = GetBinsContainersBySignal(signal, options);
                var dataPoint = new AggregationDataPoint
                {
                    Identifier = signal.LocationDescription(),
                    Value = options.SelectedAggregationType == AggregationCalculationType.Sum
                        ? binsContainers.Sum(b => b.SumValue)
                        : Convert.ToInt32(Math.Round(binsContainers.Sum(b => b.SumValue) / (double)signals.Count))
                };
                series.DataPoints.Add(dataPoint);
            }
            return series;
        }

        protected AggregationResult GetTimeXAxisRouteSeriesChart(List<Location> signals, AggregationOptions options)
        {
            var chart = new AggregationResult();
            Series series = GetTimeXAxisRouteSeries(signals, options);
            chart.Series.Add(series);
            return chart;
        }

        public Series GetTimeXAxisRouteSeries(List<Location> signals, AggregationOptions options)
        {
            var series = CreateSeries("Route");
            var binsContainers = GetBinsContainersByRoute(signals, options);
            foreach (var binsContainer in binsContainers)
            {
                foreach (var bin in binsContainer.Bins)
                {
                    series.DataPoints.Add(options.SelectedAggregationType == AggregationCalculationType.Sum
                        ? GetDataPointForSum(bin)
                        : GetDataPointForAverage(bin));
                }
            }
            return series;
        }

        protected virtual List<AggregationResult> GetTimeOfDayCharts(AggregationOptions options, List<Location> signals)
        {
            List<AggregationResult> charts = new List<AggregationResult>();
            switch (options.SelectedSeries)
            {
                case SeriesType.Signal:
                    charts.Add(GetTimeOfDayXAxisSignalSeriesChart(signals, options));
                    break;
                case SeriesType.Route:
                    charts.Add(GetTimeOfDayXAxisRouteSeriesChart(signals, options));
                    break;
                default:
                    throw new Exception("Invalid X-Axis Series Combination");
            }
            return charts;
        }

        protected AggregationResult GetTimeOfDayXAxisRouteSeriesChart(List<Location> signals, AggregationOptions options)
        {
            var chart = new AggregationResult();
            var binsContainers = GetBinsContainersByRoute(signals, options);
            var series = CreateSeries("Route");
            SetTimeAggregateSeries(series, binsContainers, options);
            chart.Series.Add(series);
            return chart;
        }


        protected AggregationResult GetTimeOfDayXAxisSignalSeriesChart(List<Location> signals, AggregationOptions options)
        {
            var chart = new AggregationResult();
            var seriesList = new ConcurrentBag<Series>();
            var binsContainers = new ConcurrentBag<BinsContainer>();
            //Parallel.For(0, signals.Count, i => // 
            for (int i = 0; i < signals.Count; i++)
            {
                var signalBinsContainers = GetBinsContainersBySignal(signals[i], options);
                binsContainers.Add(signalBinsContainers.FirstOrDefault());
                var series = CreateSeries(signals[i].LocationDescription());
                SetTimeAggregateSeries(series, signalBinsContainers, options);
                seriesList.Add(series);
            }//);
            foreach (var signal in signals)
            {
                var series = seriesList.FirstOrDefault(s => s.Identifier == signal.LocationDescription());
                chart.Series.Add(series);
            }
            return chart;
        }

        protected AggregationResult GetTimeXAxisSignalSeriesChart(List<Location> signals, AggregationOptions options)
        {
            //SetTimeXAxisAxisMinimum(chart);
            AggregationResult chart = new AggregationResult();
            var i = 1;
            foreach (var signal in signals)
            {
                Series series = GetTimeXAxisSignalSeries(signal, options);
                chart.Series.Add(series);
                i++;
            }
            return chart;
        }

        //public virtual void SetSignalsXAxisSignalSeriesForEventCount(List<Location> signals, AggregationResult chart)
        //{
        //    var eventCountOptions = new SignalEventCountAggregationOptions(this);
        //    Series eventCountSeries = eventCountOptions.GetSignalsXAxisSignalSeries(signals, "Event Count");

        //    chart.Series.Add(SetEventCountSeries(eventCountSeries));
        //}

        //public virtual void SetTimeXAxisRouteSeriesForEventCount(List<Location> signals, AggregationResult chart)
        //{
        //    var eventCountOptions = new SignalEventCountAggregationOptions(this);
        //    Series series = eventCountOptions.GetTimeXAxisRouteSeries(signals);

        //    chart.Series.Add(SetEventCountSeries(series));
        //}


        //public virtual void SetTimeOfDayAxisRouteSeriesForEventCount(List<Location> signals, AggregationResult chart)
        //{
        //    var eventCountOptions = new SignalEventCountAggregationOptions(this);
        //    Series eventCountSeries = CreateEventCountSeries();
        //    var eventBinsContainers = eventCountOptions.GetBinsContainersByRoute(signals);
        //    eventCountOptions.SetTimeAggregateSeries(eventCountSeries, eventBinsContainers);
        //    chart.Series.Add(eventCountSeries);
        //}

        public Series GetTimeXAxisSignalSeries(Location signal, AggregationOptions options)
        {
            var series = CreateSeries(signal.LocationDescription());
            var binsContainers = GetBinsContainersBySignal(signal, options);
            foreach (var container in binsContainers)
            {
                if (binsContainers.Count > 1)
                {
                    AggregationDataPoint dataPoint;
                    if (container != null)
                    {
                        if (options.SelectedAggregationType == AggregationCalculationType.Sum)
                        {
                            dataPoint = GetContainerDataPointForSum(container);
                        }
                        else
                        {
                            dataPoint = GetContainerDataPointForAverage(container);
                        }
                        series.DataPoints.Add(dataPoint);
                    }
                }
                else
                {
                    foreach (var bin in container.Bins)
                    {
                        AggregationDataPoint dataPoint;
                        if (bin != null)
                        {
                            if (options.SelectedAggregationType == AggregationCalculationType.Sum)
                            {
                                dataPoint = GetDataPointForSum(bin);
                            }
                            else
                            {
                                dataPoint = GetDataPointForAverage(bin);
                            }
                            series.DataPoints.Add(dataPoint);
                        }
                    }
                }
            }

            return series;
        }

        private void RemoveApproachesByFilter(FilterSignal filterSignal, Location signal, AggregationOptions options)
        {
            RemoveApproachesFromSignalByDirection(signal, options);
            RemoveDetectorsFromSignalByMovement(signal, options);
            var excludedApproachIds =
                filterSignal.Approaches.Where(f => f.Exclude).Select(f => f.ApproachId).ToList();
            var excludedApproaches = signal.Approaches.Where(a => excludedApproachIds.Contains(a.Id));
            foreach (var excludedApproach in excludedApproaches)
            {
                var approachesToExclude = signal.Approaches.Where(a =>
                        a.DirectionTypeId == excludedApproach.DirectionTypeId
                        && a.ProtectedPhaseNumber == excludedApproach.ProtectedPhaseNumber
                        && a.PermissivePhaseNumber == excludedApproach.PermissivePhaseNumber
                        && a.IsPermissivePhaseOverlap ==
                        excludedApproach.IsPermissivePhaseOverlap
                        && a.IsProtectedPhaseOverlap ==
                        excludedApproach.IsProtectedPhaseOverlap)
                    .ToList();
                foreach (var approachToExclude in approachesToExclude)
                    signal.Approaches.Remove(approachToExclude);
                foreach (var approach in signal.Approaches)
                    foreach (var filterApproach in filterSignal.Approaches.Where(f => !f.Exclude))
                        RemoveDetectorsFromApproachByFilter(filterApproach, approach);
            }
        }

        private void RemoveApproachesFromSignalByDirection(Location signal, AggregationOptions options)
        {
            var approachesToRemove = new List<Approach>();
            foreach (var approach in signal.Approaches)
                if (options.FilterDirections.Where(f => !f.Include).Select(f => f.DirectionTypeId).ToList()
                    .Contains((int)approach.DirectionTypeId))
                    approachesToRemove.Add(approach);
            foreach (var approach in approachesToRemove)
                signal.Approaches.Remove(approach);
        }

        private void RemoveDetectorsFromSignalByMovement(Location signal, AggregationOptions options)
        {
            var detectorsToRemove = new List<Detector>();
            foreach (var approach in signal.Approaches)
            {
                foreach (var detector in approach.Detectors)
                    if (options.FilterMovements.Where(f => !f.Include).Select(f => f.MovementTypeId).ToList()
                        .Contains((int)detector.MovementType))
                        detectorsToRemove.Add(detector);
                foreach (var detectorToRemove in detectorsToRemove)
                    approach.Detectors.Remove(detectorToRemove);
            }
        }

        private static void RemoveDetectorsFromApproachByFilter(FilterApproach filterApproach, Approach approach)
        {
            var excludedDetectorIds =
                filterApproach.Detectors.Where(f => f.Exclude).Select(f => f.Id).ToList();
            var excludedDetectors = approach.Detectors.Where(d => excludedDetectorIds.Contains(d.Id));
            foreach (var excludedDetector in excludedDetectors)
            {
                var detectorsToExclude = approach.Detectors.Where(d =>
                    d.LaneNumber == excludedDetector.LaneNumber
                    && d.LaneType == excludedDetector.LaneType
                    && d.MovementType == excludedDetector.MovementType
                    && d.DetectionHardware == excludedDetector.DetectionHardware
                    && d.DetectorChannel == excludedDetector.DetectorChannel
                ).ToList();
                foreach (var detectorToExclude in detectorsToExclude)
                    approach.Detectors.Remove(detectorToExclude);
            }
        }

        protected AggregationDataPoint GetDataPointForSum(Bin bin)
        {
            var dataPoint = new AggregationDataPoint { Start = bin.Start, Value = bin.Sum };
            //dataPoint.Label = bin.Start.Month.ToString();
            return dataPoint;
        }

        protected AggregationDataPoint GetDataPointForAverage(Bin bin)
        {
            var dataPoint = new AggregationDataPoint { Start = bin.Start, Value = bin.Average };
            return dataPoint;
        }

        protected AggregationDataPoint GetContainerDataPointForSum(BinsContainer bin)
        {
            var dataPoint = new AggregationDataPoint { Start = bin.Start, Value = bin.SumValue };
            return dataPoint;
        }

        protected AggregationDataPoint GetContainerDataPointForAverage(BinsContainer bin)
        {
            var dataPoint = new AggregationDataPoint { Start = bin.Start, Value = bin.AverageValue };
            return dataPoint;
        }

        public void SetTimeAggregateSeries(Series series, List<BinsContainer> binsContainers, AggregationOptions options)
        {
            SetEndTimeAndMinutes(out var endTime, out var minutes, options);
            SetDataPointsForTimeAggregationSeries(binsContainers, series, endTime, minutes, options);
        }


        private void SetEndTimeAndMinutes(out DateTime endTime, out int minutes, AggregationOptions options)
        {
            endTime = new DateTime(options.TimeOptions.Start.Year, options.TimeOptions.Start.Month, options.TimeOptions.Start.Day,
                            options.TimeOptions.TimeOfDayEndHour ?? 0, options.TimeOptions.TimeOfDayEndMinute ?? 0, 0);
            switch (options.TimeOptions.SelectedBinSize)
            {
                case TimeOptions.BinSize.FifteenMinute:
                    minutes = 15;
                    break;
                case TimeOptions.BinSize.ThirtyMinute:
                    minutes = 30;
                    break;
                case TimeOptions.BinSize.Hour:
                    minutes = 60;
                    break;
                case TimeOptions.BinSize.Day:
                    minutes = 60 * 24;
                    break;
                case TimeOptions.BinSize.Month:
                    minutes = 60 * 24 * 30;
                    break;
                case TimeOptions.BinSize.Year:
                    minutes = 60 * 24 * 365;
                    break;
                default:
                    throw new InvalidBinSizeException(options.TimeOptions.SelectedBinSize.ToString() + " is an invalid bin size for time period aggregation");
            }
        }

        private void SetDataPointsForTimeAggregationSeries(List<BinsContainer> binsContainers, Series series,
            DateTime endTime, int minutes, AggregationOptions options)
        {
            switch (options.TimeOptions.SelectedBinSize)
            {
                case TimeOptions.BinSize.Year:
                    {
                        foreach (var binContainer in binsContainers)
                        {
                            if (options.SelectedAggregationType == AggregationCalculationType.Sum)
                            {
                                series.DataPoints.Add(new AggregationDataPoint { Start = binContainer.Start.Date, Value = binContainer.SumValue });
                            }
                            else
                            {
                                series.DataPoints.Add(new AggregationDataPoint { Start = binContainer.Start.Date, Value = binContainer.AverageValue });
                            }
                        }
                        break;
                    }
                case TimeOptions.BinSize.Month:
                    {
                        foreach (var binContainer in binsContainers)
                        {
                            if (options.SelectedAggregationType == AggregationCalculationType.Sum)
                            {
                                series.DataPoints.Add(new AggregationDataPoint { Start = binContainer.Start.Date, Value = binContainer.SumValue });
                            }
                            else
                            {
                                series.DataPoints.Add(new AggregationDataPoint { Start = binContainer.Start.Date, Value = binContainer.AverageValue });
                            }
                        }
                        break;
                    }
                case TimeOptions.BinSize.Day:
                    {
                        for (var startTime = new DateTime(options.TimeOptions.Start.Year, options.TimeOptions.Start.Month,
                                options.TimeOptions.Start.Day,
                                options.TimeOptions.TimeOfDayStartHour ?? 0, options.TimeOptions.TimeOfDayStartMinute ?? 0, 0);
                            startTime.Date <= options.TimeOptions.End.Date;
                            startTime = startTime.AddMinutes(minutes))
                        {
                            if (options.SelectedAggregationType == AggregationCalculationType.Sum)
                            {
                                var sumValue = binsContainers.FirstOrDefault().Bins.Where(b =>
                                    b.Start.Date == startTime.Date).Sum(b => b.Sum);
                                series.DataPoints.Add(new AggregationDataPoint { Start = startTime.Date, Value = sumValue });
                            }
                            else
                            {
                                double averageValue = 0;
                                if (binsContainers.FirstOrDefault().Bins.Any(b =>
                                    b.Start.Date == startTime.Date))
                                    averageValue = binsContainers.FirstOrDefault().Bins.Where(b =>
                                            b.Start.Date == startTime.Date)
                                        .Average(b => b.Sum);
                                series.DataPoints.Add(new AggregationDataPoint { Start = startTime.Date, Value = averageValue });
                            }
                        }
                        break;
                    }
                default:
                    {
                        for (var startTime = new DateTime(options.TimeOptions.Start.Year, options.TimeOptions.Start.Month,
                                options.TimeOptions.Start.Day,
                                options.TimeOptions.TimeOfDayStartHour ?? 0, options.TimeOptions.TimeOfDayStartMinute ?? 0, 0);
                            startTime < endTime;
                            startTime = startTime.AddMinutes(minutes))
                        {
                            if (options.SelectedAggregationType == AggregationCalculationType.Sum)
                            {
                                var sumValue = binsContainers.FirstOrDefault().Bins.Where(b =>
                                    b.Start.Hour == startTime.Hour && b.Start.Minute == startTime.Minute).Sum(b => b.Sum);
                                series.DataPoints.Add(new AggregationDataPoint { Start = startTime, Value = sumValue });
                            }
                            else
                            {
                                double averageValue = 0;
                                if (binsContainers.FirstOrDefault().Bins.Any(b =>
                                    b.Start.Hour == startTime.Hour && b.Start.Minute == startTime.Minute))
                                    averageValue = binsContainers.FirstOrDefault().Bins.Where(b =>
                                            b.Start.Hour == startTime.Hour && b.Start.Minute == startTime.Minute)
                                        .Average(b => b.Sum);
                                series.DataPoints.Add(new AggregationDataPoint { Start = startTime, Value = averageValue });
                            }
                        }
                        break;
                    }
            }
        }

        protected Series CreateSeries(string seriesName)
        {
            var series = new Series { Identifier = seriesName };
            return series;
        }

        //protected Series CreateEventCountSeries()
        //{
        //    Series series = new Series();
        //    return SetEventCountSeries(series);
        //}

        //public Series SetEventCountSeries(Series series)
        //{
        //    series.BorderWidth = SeriesWidth;
        //    series.Color = GetSeriesColorByNumber(-1);
        //    series.Name = "Event Count";
        //    series.ChartArea = "ChartArea1";
        //    series.ChartType = SeriesChartType.Line;
        //    series.YAxisType = AxisType.Secondary;
        //    return series;
        //}




        protected static void PopulateBinsForRoute(List<Location> signals, List<BinsContainer> binsContainers, AggregationBySignal aggregationBySignal)
        {
            for (var i = 0; i < binsContainers.Count; i++)
            {
                for (var binIndex = 0; binIndex < binsContainers[i].Bins.Count; binIndex++)
                {
                    var bin = binsContainers[i].Bins[binIndex];
                    bin.Sum += aggregationBySignal.BinsContainers[i].Bins[binIndex].Sum;
                    bin.Average = Convert.ToInt32(Math.Round((double)(bin.Sum / signals.Count)));
                }
            }
        }

        protected abstract List<BinsContainer> GetBinsContainersBySignal(Location signal, AggregationOptions options);
        public abstract List<BinsContainer> GetBinsContainersByRoute(List<Location> signals, AggregationOptions options);
    }

    public class InvalidBinSizeException : Exception
    {
        public InvalidBinSizeException()
        {
        }

        public InvalidBinSizeException(string message)
            : base(message)
        {
        }

        public InvalidBinSizeException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}