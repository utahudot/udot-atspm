#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - MOE.Common.Business.WCFServiceLibrary/PhaseAggregationMetricOptions.cs
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
            var signals = GetSignalObjects(options);
            switch (options.SelectedXAxisType)
            {
                case XAxisType.Time:
                    return GetTimeCharts(options, signals);
                case XAxisType.TimeOfDay:
                    return GetTimeOfDayCharts(options, signals);
                case XAxisType.Approach:
                    return GetPhaseCharts(options, signals);
                case XAxisType.Signal:
                    return GetSignalCharts(options, signals);
                default:
                    throw new Exception("Invalid X-Axis");
            }
        }

        protected List<AggregationResult> GetPhaseCharts(AggregationOptions options, List<Location> signals)
        {
            var charts = new List<AggregationResult>();
            switch (options.SelectedSeries)
            {
                case SeriesType.PhaseNumber:
                    var chart = new AggregationResult();
                    foreach (var signal in signals)
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

        protected override List<AggregationResult> GetTimeCharts(AggregationOptions options, List<Location> signals)
        {
            var charts = new List<AggregationResult>();
            switch (options.SelectedSeries)
            {
                case SeriesType.PhaseNumber:
                    foreach (var signal in signals)
                    {
                        var availablePhaseNumbers = GetAvailablePhaseNumbers(signal, options);
                        charts.Add(GetTimeXAxisApproachSeriesChart(signal, availablePhaseNumbers, options));
                    }
                    break;
                case SeriesType.Signal:
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
            switch (options.SelectedSeries)
            {
                case SeriesType.PhaseNumber:
                    charts.Add(GetSignalsXAxisPhaseNumberSeriesChart(signals, options));
                    break;
                case SeriesType.Signal:
                    var chart = new AggregationResult();
                    chart.Series.Add(GetSignalsXAxisSignalSeries(signals, options));
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


        protected override List<AggregationResult> GetTimeOfDayCharts(AggregationOptions options, List<Location> signals)
        {
            var charts = new List<AggregationResult>();
            switch (options.SelectedSeries)
            {
                case SeriesType.PhaseNumber:
                    foreach (var signal in signals)
                    {
                        charts.Add(GetTimeOfDayXAxisPhaseSeriesChart(signal, options));
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
                var dataPoint = new AggregationDataPoint();
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