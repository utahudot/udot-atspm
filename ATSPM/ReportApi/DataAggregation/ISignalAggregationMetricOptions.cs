#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - MOE.Common.Business.WCFServiceLibrary/ISignalAggregationMetricOptions.cs
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
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.ReportApi.DataAggregation;

namespace MOE.Common.Business.WCFServiceLibrary
{
    public interface ISignalAggregationMetricOptions
    {
        List<AggregationResult> CreateMetric(AggregationOptions options);
        List<BinsContainer> GetBinsContainersByRoute(List<Location> signals, AggregationOptions options);
        List<DirectionTypes> GetFilteredDirections(AggregationOptions options);
        Series GetSignalsXAxisSignalSeries(List<Location> signals, string seriesName, AggregationOptions options);
        Series GetTimeXAxisRouteSeries(List<Location> signals, AggregationOptions options);
        Series GetTimeXAxisSignalSeries(Location signal, AggregationOptions options);
        void SetTimeAggregateSeries(Series series, List<BinsContainer> binsContainers, AggregationOptions options);
    }
}