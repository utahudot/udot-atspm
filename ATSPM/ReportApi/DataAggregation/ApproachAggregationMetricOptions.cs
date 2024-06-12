#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - MOE.Common.Business.WCFServiceLibrary/ApproachAggregationMetricOptions.cs
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
using ATSPM.Application.Business.Common;
using ATSPM.Application.Enums;
using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.TempExtensions;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.ReportApi.DataAggregation;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace MOE.Common.Business.WCFServiceLibrary
{

    public abstract class ApproachAggregationMetricOptions : SignalAggregationMetricOptions
    {

        protected ApproachAggregationMetricOptions(ILocationRepository locationRepository, ILogger logger) : base(locationRepository, logger)
        {
        }

        protected override List<AggregationResult> GetChartByXAxisAggregation(AggregationOptions options)
        {
            var signals = GetSignalObjects(options);
            switch (options.SelectedXAxisType)
            {
                case XAxisType.Time:
                    return GetTimeCharts(options, signals);
                case XAxisType.TimeOfDay:
                    return GetTimeOfDayCharts(options, signals);
                case XAxisType.Approach:
                    return GetApproachCharts(options, signals);
                case XAxisType.Direction:
                    return GetDirectionCharts(options, signals);
                case XAxisType.Signal:
                    return GetSignalCharts(options, signals);
                default:
                    throw new Exception("Invalid X-Axis");
            }
        }


        protected List<AggregationResult> GetDirectionCharts(AggregationOptions options, List<Location> signals)
        {
            var charts = new List<AggregationResult>();
            AggregationResult chart = new AggregationResult();
            charts.Add(chart);
            switch (options.SelectedSeries)
            {
                case SeriesType.Direction:
                    foreach (var signal in signals)
                    {
                        chart.Series.Add(GetDirectionXAxisDirectionSeries(signal, options));
                    }
                    break;
                default:
                    throw new Exception("Invalid X-Axis Series Combination");
            }
            return charts;
        }

        private void GetDirectionXAxisDirectionSeries(Location signal, AggregationResult chart, AggregationOptions options)
        {
            Series series = GetDirectionXAxisDirectionSeries(signal, options);
            chart.Series.Add(series);
        }

        public virtual Series GetDirectionXAxisDirectionSeries(Location signal, AggregationOptions options)
        {
            var series = CreateSeries(signal.LocationDescription());
            var directionsList = GetFilteredDirections(options);
            foreach (var direction in directionsList)
            {
                var dataPoint = new AggregationDataPoint { Identifier = direction.GetDescription() };
                if (options.SelectedAggregationType == AggregationCalculationType.Sum)
                    dataPoint.Value = GetSumByDirection(signal, direction, options);
                else
                    dataPoint.Value = GetAverageByDirection(signal, direction, options);
                series.DataPoints.Add(dataPoint);
            }

            return series;
        }

        protected List<AggregationResult> GetApproachCharts(AggregationOptions options, List<Location> signals)
        {
            var charts = new List<AggregationResult>();
            switch (options.SelectedSeries)
            {
                case SeriesType.PhaseNumber:
                    foreach (var signal in signals)
                    {
                        charts.Add(GetApproachXAxisChart(signal, options));
                    }
                    break;
                default:
                    throw new Exception("Invalid X-Axis Series Combination");
            }
            return charts;
        }


        protected override List<AggregationResult> GetTimeCharts(AggregationOptions options, List<Location> signals)
        {
            var charts = new List<AggregationResult>();
            switch (options.SelectedSeries)
            {
                case SeriesType.PhaseNumber:
                    foreach (var signal in signals)
                    {
                        //chart = ChartFactory.CreateTimeXIntYChart(this, new List<Location> { signal });
                        charts.Add(GetTimeXAxisApproachSeriesChart(signal, options));
                    }
                    break;
                case SeriesType.Direction:
                    foreach (var signal in signals)
                    {
                        //chart = ChartFactory.CreateTimeXIntYChart(this, new List<Location> { signal });
                        charts.Add(GetTimeXAxisDirectionSeriesChart(signal, options));
                    }
                    ;
                    break;
                case SeriesType.Signal:
                    //chart = ChartFactory.CreateTimeXIntYChart(this, Signals);
                    charts.Add(GetTimeXAxisSignalSeriesChart(signals, options));
                    break;
                case SeriesType.Route:
                    charts.Add(GetTimeXAxisRouteSeriesChart(signals, options));
                    break;
                default:
                    throw new Exception("Invalid X-Axis Series Combination");
            }
            return charts;
        }


        protected override List<AggregationResult> GetSignalCharts(AggregationOptions options, List<Location> signals)
        {
            var charts = new List<AggregationResult>();
            AggregationResult chart = new AggregationResult();
            charts.Add(chart);
            switch (options.SelectedSeries)
            {
                case SeriesType.PhaseNumber:
                    chart.Series.AddRange(GetSignalsXAxisPhaseNumberSeries(signals, options));
                    break;
                case SeriesType.Direction:
                    chart.Series.AddRange(GetSignalsXAxisDirectionSeries(signals, options));
                    break;
                case SeriesType.Signal:
                    chart.Series.Add(GetSignalsXAxisSignalSeries(signals, options));
                    break;
                default:
                    throw new Exception("Invalid X-Axis Series Combination");
            }
            return charts;
        }


        protected List<Series> GetSignalsXAxisDirectionSeries(List<Location> signals, AggregationOptions options)
        {
            var seriesList = new List<Series>();
            var availableDirections = new List<DirectionTypes>();
            foreach (var signal in signals)
                availableDirections.AddRange(signal.GetAvailableDirections());
            availableDirections = availableDirections.Distinct().ToList();
            foreach (var directionType in availableDirections)
            {
                var seriesName = directionType.GetDescription();
                var series = CreateSeries(seriesName);
                foreach (var signal in signals)
                {
                    var binsContainers = GetBinsContainersByDirection(directionType, signal, options);
                    var dataPoint = new AggregationDataPoint
                    {
                        Identifier = signal.LocationDescription(),
                        Value = options.SelectedAggregationType == AggregationCalculationType.Sum
                        ? binsContainers.Sum(b => b.SumValue)
                        : Convert.ToInt32(Math.Round(binsContainers.Sum(b => b.SumValue) /
                                                     (double)availableDirections.Count))
                    };
                    series.DataPoints.Add(dataPoint);
                }
                seriesList.Add(series);
            }
            return seriesList;
        }


        protected AggregationResult GetTimeXAxisDirectionSeriesChart(Location signal, AggregationOptions options)
        {
            //var charts = new List<AggregationResult>();
            var chart = new AggregationResult();
            //charts.Add(chart);
            foreach (var directionType in signal.GetAvailableDirections().Distinct().ToList())
            {
                GetDirectionSeries(chart, directionType, signal, options);
            }
            return chart;
        }

        private void GetDirectionSeries(AggregationResult chart, DirectionTypes directionType, Location signal, AggregationOptions options)
        {
            var series = CreateSeries(directionType.GetDescription());
            var binsContainers = GetBinsContainersByDirection(directionType, signal, options);
            foreach (var binsContainer in binsContainers)
                foreach (var bin in binsContainer.Bins)
                {
                    var dataPoint = options.SelectedAggregationType == AggregationCalculationType.Sum
                        ? GetDataPointForSum(bin)
                        : GetDataPointForAverage(bin);
                    series.DataPoints.Add(dataPoint);
                }
            chart.Series.Add(series);
        }


        protected override List<AggregationResult> GetTimeOfDayCharts(AggregationOptions options, List<Location> signals)
        {
            var charts = new List<AggregationResult>();
            switch (options.SelectedSeries)
            {
                case SeriesType.PhaseNumber:
                    foreach (var signal in signals)
                    {
                        charts.Add(GetTimeOfDayXAxisApproachSeriesChart(signal, options));
                    }
                    break;
                case SeriesType.Direction:
                    foreach (var signal in signals)
                    {
                        charts.Add(GetTimeOfDayXAxisDirectionSeriesChart(signal, options));
                    }
                    break;
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


        private void SetTimeofDayAxisSignalSeriesForEventCount(Location signal, AggregationResult chart)
        {
            //var eventCountOptions = new  ApproachEventCountAggregationOptions(this);
            //var binsContainers = eventCountOptions.GetBinsContainersByRoute(new List<Location> { signal });
            //Series series = CreateEventCountSeries();
            //eventCountOptions.SetTimeAggregateSeries(series, binsContainers);
            //chart.Series.Add(series);
        }


        protected AggregationResult GetTimeOfDayXAxisDirectionSeriesChart(Location signal, AggregationOptions options)
        {
            var chart = new AggregationResult();
            var availableDirections = signal.GetAvailableDirections().ToList();
            if (availableDirections == null) return chart;
            var seriesList = new ConcurrentBag<Series>();
            Parallel.For(0, availableDirections.Count, i => // foreach (var signal in signals)
            {
                var binsContainers = GetBinsContainersByDirection(availableDirections[i], signal, options);
                var series = CreateSeries(availableDirections[i].GetDescription());
                try
                {
                    SetTimeAggregateSeries(series, binsContainers, options);
                    seriesList.Add(series);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    throw;
                }
            });
            foreach (var direction in availableDirections)
                chart.Series.Add(seriesList.FirstOrDefault(s => s.Identifier == direction.GetDescription()));

            return chart;
        }


        protected AggregationResult GetTimeOfDayXAxisApproachSeriesChart(Location signal, AggregationOptions options)
        {
            var chart = new AggregationResult();
            var seriesList = new ConcurrentBag<Series>();
            var approaches = signal.Approaches.ToList();
            try
            {


                Parallel.For(0, approaches.Count, i =>
                {
                    var phaseDescription = GetPhaseDescription(approaches[i], true);
                    var binsContainers = GetBinsContainersByApproach(approaches[i], true, options);
                    var series = CreateSeries(approaches[i].Description + phaseDescription);
                    SetTimeAggregateSeries(series, binsContainers, options);
                    seriesList.Add(series);
                    if (approaches[i].PermissivePhaseNumber != null)
                    {
                        var permissivePhaseDescription = GetPhaseDescription(approaches[i], false);
                        var permissiveBinsContainers = GetBinsContainersByApproach(approaches[i], false, options);
                        var permissiveSeries = CreateSeries(approaches[i].Description + permissivePhaseDescription);
                        SetTimeAggregateSeries(permissiveSeries, permissiveBinsContainers, options);
                        seriesList.Add(permissiveSeries);
                        i++;
                    }
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


        protected List<Series> GetSignalsXAxisPhaseNumberSeries(List<Location> signals, AggregationOptions options)
        {
            var seriesList = new List<Series>();
            var availablePhaseNumbers = new List<int>();
            foreach (var signal in signals)
                availablePhaseNumbers.AddRange(signal.GetPhasesForSignal());
            availablePhaseNumbers = availablePhaseNumbers.Distinct().ToList();
            foreach (var phaseNumber in availablePhaseNumbers)
            {
                var seriesName = "Phase " + phaseNumber;
                Series series = GetSignalXAxisPhaseNumberSeries(signals, phaseNumber, seriesName, options);
                seriesList.Add(series);
            }
            return seriesList;
        }

        public Series GetSignalXAxisPhaseNumberSeries(List<Location> signals, int phaseNumber, string seriesName, AggregationOptions options)
        {
            var series = CreateSeries(seriesName);
            foreach (var signal in signals)
            {
                var binsContainers = GetBinsContainersByPhaseNumber(signal, phaseNumber, options);
                var dataPoint = new AggregationDataPoint
                {
                    Identifier = signal.LocationDescription(),
                    Value = options.SelectedAggregationType == AggregationCalculationType.Sum
                    ? binsContainers.Sum(b => b.SumValue)
                    : binsContainers.Average(b => b.SumValue)
                };
                series.DataPoints.Add(dataPoint);
            }

            return series;
        }

        protected AggregationResult GetApproachXAxisChart(Location signal, AggregationOptions options)
        {
            var chart = new AggregationResult();
            Series series = GetApproachXAxisApproachSeries(signal, options);
            chart.Series.Add(series);
            return chart;
        }

        private Series GetApproachXAxisApproachSeries(Location signal, AggregationOptions options)
        {
            var series = CreateSeries(signal.LocationDescription());
            foreach (var approach in signal.Approaches)
            {
                var binsContainers = GetBinsContainersByApproach(approach, true, options);
                var dataPoint = new AggregationDataPoint();
                if (options.SelectedAggregationType == AggregationCalculationType.Sum)
                    dataPoint.Value = binsContainers.FirstOrDefault().SumValue;
                else
                    dataPoint.Value = binsContainers.FirstOrDefault().AverageValue;

                dataPoint.Identifier = approach.Description;
                series.DataPoints.Add(dataPoint);
                if (approach.PermissivePhaseNumber != null)
                {
                    var binsContainers2 = GetBinsContainersByApproach(approach, false, options);
                    var dataPoint2 = new AggregationDataPoint();
                    if (options.SelectedAggregationType == AggregationCalculationType.Sum)
                        dataPoint2.Value = binsContainers2.FirstOrDefault().SumValue;
                    else
                        dataPoint2.Value = binsContainers2.FirstOrDefault().AverageValue;
                    dataPoint2.Identifier = approach.Description;
                    series.DataPoints.Add(dataPoint2);
                }
            }

            return series;
        }

        protected void GetPhaseXAxisChart(Location signal, AggregationResult chart, AggregationOptions options)
        {
            Series series = GetPhaseXAxisPhaseSeries(signal, options);
            chart.Series.Add(series);
        }

        public Series GetPhaseXAxisPhaseSeries(Location signal, AggregationOptions options)
        {
            var series = CreateSeries(signal.LocationDescription());
            var phaseNumbers = signal.GetPhasesForSignal();
            foreach (var phaseNumber in phaseNumbers)
            {
                var dataPoint = new AggregationDataPoint();
                if (options.SelectedAggregationType == AggregationCalculationType.Sum)
                    dataPoint.Value = GetSumByPhaseNumber(signal, phaseNumber, options);
                else
                    dataPoint.Value = GetAverageByPhaseNumber(signal, phaseNumber, options);
                dataPoint.Identifier = "Phase " + phaseNumber;
                series.DataPoints.Add(dataPoint);
            }
            return series;
        }

        protected AggregationResult GetTimeXAxisApproachSeriesChart(Location signal, AggregationOptions options)
        {
            var chart = new AggregationResult();
            var i = 1;
            foreach (var approach in signal.Approaches)
            {
                GetApproachTimeSeriesByProtectedPermissive(chart, approach, true, options);
                i++;
                if (approach.PermissivePhaseNumber != null)
                {
                    GetApproachTimeSeriesByProtectedPermissive(chart, approach, false, options);
                    i++;
                }
            }
            return chart;
        }

        private static string GetPhaseDescription(Approach approach, bool getProtectedPhase)
        {
            return getProtectedPhase
                ? " Phase " + approach.ProtectedPhaseNumber
                : " Phase " + approach.PermissivePhaseNumber;
        }

        private void GetApproachTimeSeriesByProtectedPermissive(AggregationResult chart, Approach approach,
            bool getProtectedPhase, AggregationOptions options)
        {
            Series series = GetTimeXAxisPhaseSeries(approach, getProtectedPhase, options);
            chart.Series.Add(series);
        }

        private Series GetTimeXAxisPhaseSeries(Approach approach, bool getProtectedPhase, AggregationOptions options)
        {
            var phaseDescription = GetPhaseDescription(approach, getProtectedPhase);
            var binsContainers = GetBinsContainersByApproach(approach, getProtectedPhase, options);
            var series = CreateSeries(approach.Description
                + " - PH " + (getProtectedPhase ? approach.ProtectedPhaseNumber.ToString() : approach.PermissivePhaseNumber.Value.ToString()));
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

        protected abstract List<BinsContainer> GetBinsContainersByApproach(Approach approach, bool getprotectedPhase, AggregationOptions options);
        protected abstract int GetAverageByPhaseNumber(Location signal, int phaseNumber, AggregationOptions options);
        protected abstract double GetSumByPhaseNumber(Location signal, int phaseNumber, AggregationOptions options);
        protected abstract int GetAverageByDirection(Location signal, DirectionTypes direction, AggregationOptions options);
        protected abstract double GetSumByDirection(Location signal, DirectionTypes direction, AggregationOptions options);
        protected abstract List<BinsContainer> GetBinsContainersByDirection(DirectionTypes directionType, Location signal, AggregationOptions options);
        protected abstract List<BinsContainer> GetBinsContainersByPhaseNumber(Location signal, int phaseNumber, AggregationOptions options);

    }
}