#region license
// Copyright 2026 Utah Departement of Transportation
// for ReportApi - Utah.Udot.Atspm.ReportApi.ReportServices/TurningMovementCountReportService.cs
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

using Utah.Udot.Atspm.Exceptions;
using Utah.Udot.Atspm.Business.TurningMovementCounts;

namespace Utah.Udot.Atspm.ReportApi.ReportServices;

public sealed class TurningMovementCountReportService(ILocationRepository locations, TmcCountSourceResolver resolver,
    TmcResultBuilder builder) : ReportServiceBase<TurningMovementCountsOptions, TurningMovementCountsResult>
{
    public override async Task<TurningMovementCountsResult> ExecuteAsync(TurningMovementCountsOptions options,
        IProgress<int> progress = null, CancellationToken cancelToken = default)
    {
        if (options.BinSize < 1 || options.End <= options.Start)
            throw new ReportException(400, "Choose a positive bin size and an end time after the start time.");
        if (options.Source == "devices" && (options.BinSize > 1440 || (options.End - options.Start).TotalDays > 31))
            throw new ReportException(400, "Choose a bin size from 1 to 1440 minutes and a date range of up to 31 days.");
        var location = locations.GetLatestVersionOfLocation(options.LocationIdentifier, options.Start)
            ?? throw new ReportException(400, "Location not found");
        var (source, request) = resolver.Resolve(location, options);
        var data = await source.ReadAsync(request, cancelToken);
        if (options.ReconcileLanes)
            return new TurningMovementCountsResult { Source = data.Label, Warnings = data.Warnings,
                ZoneEvidence = data.ZoneEvidence, SourceDeviceIds = data.SourceDeviceIds, Charts = new(), Table = new() };
        var plans = data.Plans.Count > 0 ? data.Plans : new[] { new Plan("0", options.Start, options.End) };
        return builder.Build(location, options, data.Counts, plans, data.Label, data.Warnings, data.BinOrigin);
    }
}
