#region license
// Copyright 2026 Utah Departement of Transportation
// for ReportApi - Utah.Udot.Atspm.ReportApi.DataAggregation/SignalAggregationMetricOptions.cs
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

using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using Utah.Udot.Atspm.Business.Aggregation.FilterExtensions;
using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.ReportApi.DataAggregation
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
            SetTimeOfDayWindow(options);
            return GetChartByXAxisAggregation(options);
        }

        // A Time Of Day chart needs a time of day, so a request over the whole range becomes
        // one covering every hour of the chosen days.
        internal static void SetTimeOfDayWindow(AggregationOptions options)
        {
            if (options.TimeOptions != null &&
                options.SelectedXAxisType == XAxisType.TimeOfDay &&
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
            // Without direction filters every direction is charted, as when the page checks them all.
            if (options.FilterDirections == null)
                return enumValues.Where(d => d != DirectionTypes.NA).ToList();
            var includedDirections = options.FilterDirections.Where(f => f.Include).Select(f => f.DirectionTypeId).ToList();
            var directionsList = enumValues.Where(d => includedDirections.Contains((int)d)).ToList();
            return directionsList;
        }


        protected List<Location> GetSignalObjects(AggregationOptions options)
        {
            var locations = new List<Location>();
            try
            {
                foreach (var filterSignal in options.Locations)
                    if (!filterSignal.Exclude)
                    {
                        // The version in effect on Start, as the other reports use, so a location
                        // with several versions is charted once.
                        var signal = locationRepository.GetLatestVersionOfLocation(filterSignal.LocationIdentifier, options.TimeOptions.Start);
                        if (signal == null)
                            continue;
                        RemoveApproachesByFilter(filterSignal, signal, options);
                        signal.Approaches = signal.Approaches.OrderBy(a => a.ProtectedPhaseNumber).ToList();
                        locations.Add(signal);
                    }
            }
            catch (Exception e)
            {
                logger.LogError($"Unable to apply signal filter {e.Message}");
                throw new Exception("Unable to apply signal filter");
            }
            // The report is open to any caller, so a request that finds no location is turned
            // away with a reason instead of charting nothing as zeros.
            if (!locations.Any())
                throw new ArgumentException("None of the selected locations were found for the chosen Start and End.");
            return locations;
        }


        protected virtual List<AggregationResult> GetTimeCharts(AggregationOptions options, List<Location> signals)
        {
            AggregationResult chart;
            switch (options.SelectedSeries)
            {
                case SeriesType.Signal:
                    //chart = ChartFactory.CreateTimeXIntYChart(this, Signals);
                    return new List<AggregationResult> { ForLocations(GetTimeXAxisSignalSeriesChart(signals, options), signals) };
                case SeriesType.Route:
                    //chart = ChartFactory.CreateTimeXIntYChart(this, Signals);
                    return new List<AggregationResult> { ForLocations(GetTimeXAxisRouteSeriesChart(signals, options), signals) };
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
            charts.Add(ForLocations(chart, signals));
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
                        : Convert.ToInt32(Math.Round(binsContainers.Sum(b => b.SumValue) / signals.Count))
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
                    charts.Add(ForLocations(GetTimeOfDayXAxisSignalSeriesChart(signals, options), signals));
                    break;
                case SeriesType.Route:
                    charts.Add(ForLocations(GetTimeOfDayXAxisRouteSeriesChart(signals, options), signals));
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

        // Exclusions are picked on the latest version of the location, which can differ from the
        // version in effect on Start, so each one is matched by what it is: an approach by its
        // direction and phases, a detector by its channel.
        private void RemoveApproachesByFilter(FilterSignal filterSignal, Location signal, AggregationOptions options)
        {
            var excludedApproachIds =
                filterSignal.Approaches.Where(f => f.Exclude).Select(f => f.ApproachId).ToList();
            var excludedDetectorIds = filterSignal.Approaches.SelectMany(f => f.Detectors)
                .Where(f => f.Exclude).Select(f => f.Id).ToList();
            var versions = GetVersionsWithExclusions(signal, excludedApproachIds, excludedDetectorIds);
            var excludedApproaches = versions.SelectMany(v => v.Approaches)
                .Where(a => excludedApproachIds.Contains(a.Id)).ToList();
            var excludedChannels = versions.SelectMany(v => v.Approaches).SelectMany(a => a.Detectors)
                .Where(d => excludedDetectorIds.Contains(d.Id)).Select(d => d.DetectorChannel).ToList();

            RemoveApproachesFromSignalByDirection(signal, options);
            RemoveDetectorsFromSignalByMovement(signal, options);
            foreach (var approach in signal.Approaches.Where(a => excludedApproaches.Any(e => IsSameApproach(a, e))).ToList())
                signal.Approaches.Remove(approach);
            foreach (var approach in signal.Approaches)
                foreach (var detector in approach.Detectors.Where(d => excludedChannels.Contains(d.DetectorChannel)).ToList())
                    approach.Detectors.Remove(detector);
        }

        // The latest version is fetched only when an exclusion names an approach or detector that
        // the version in effect on Start does not have, and before anything is removed.
        private List<Location> GetVersionsWithExclusions(Location signal, List<int> approachIds, List<int> detectorIds)
        {
            var versions = new List<Location> { signal };
            var approaches = signal.Approaches.ToList();
            var detectors = approaches.SelectMany(a => a.Detectors).ToList();
            if (approachIds.All(id => approaches.Any(a => a.Id == id)) &&
                detectorIds.All(id => detectors.Any(d => d.Id == id)))
                return versions;
            var latest = locationRepository.GetLatestVersionOfLocation(signal.LocationIdentifier);
            if (latest != null && latest.Id != signal.Id)
                versions.Add(latest);
            return versions;
        }

        private static bool IsSameApproach(Approach approach, Approach other) =>
            approach.DirectionTypeId == other.DirectionTypeId
            && approach.ProtectedPhaseNumber == other.ProtectedPhaseNumber
            && approach.PermissivePhaseNumber == other.PermissivePhaseNumber
            && approach.IsPermissivePhaseOverlap == other.IsPermissivePhaseOverlap
            && approach.IsProtectedPhaseOverlap == other.IsProtectedPhaseOverlap;

        private void RemoveApproachesFromSignalByDirection(Location signal, AggregationOptions options)
        {
            // A request without direction filters removes no approach.
            if (options.FilterDirections == null)
                return;
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
            // A request without movement filters removes no detector.
            if (options.FilterMovements == null)
                return;
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
            // A time period that ends at 00:00 runs to the end of the day, as its bins do.
            endTime = options.TimeOptions.Start.Date.Add(BinFactory.GetTimeOfDayEnd(
                options.TimeOptions.TimeOfDayEndHour ?? 0, options.TimeOptions.TimeOfDayEndMinute ?? 0));
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
            if (binsContainers is null)
            {
                throw new ArgumentNullException(nameof(binsContainers));
            }

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
                            startTime.Date < options.TimeOptions.End;
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

        // Tags a chart with the locations its data covers, so callers can name it without relying on chart order.
        internal static AggregationResult ForLocations(AggregationResult chart, IEnumerable<Location> locations)
        {
            chart.LocationIdentifiers = locations.Select(location => location.LocationIdentifier).Distinct().ToList();
            return chart;
        }

        internal static AggregationResult ForLocations(AggregationResult chart, Location location) =>
            ForLocations(chart, new[] { location });

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