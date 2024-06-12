#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - ATSPM.ReportApi.ReportServices/LeftTurnPeakHourService.cs
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
using ATSPM.Application.Business;
using ATSPM.Application.Business.LeftTurnGapReport;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Application.Repositories.ConfigurationRepositories;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Left turn gap analysis report service
    /// </summary>
    public class LeftTurnPeakHourService : ReportServiceBase<PeakHourOptions, PeakHourResult>
    {
        private readonly IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository;
        private readonly ILocationRepository locationRepository;
        private readonly LeftTurnReportService leftTurnReportService;
        private readonly ILogger<LeftTurnPeakHourService> logger;

        /// <inheritdoc/>
        public LeftTurnPeakHourService(
            IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository,
            ILocationRepository locationRepository,
            LeftTurnReportService leftTurnReportPreCheckService,
            ILogger<LeftTurnPeakHourService> logger)
        {
            this.detectorEventCountAggregationRepository = detectorEventCountAggregationRepository;
            this.locationRepository = locationRepository;
            this.leftTurnReportService = leftTurnReportPreCheckService;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public override async Task<PeakHourResult> ExecuteAsync(PeakHourOptions options, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var amStartTime = new TimeSpan(6, 0, 0);
            var amEndTime = new TimeSpan(9, 0, 0);
            var pmStartTime = new TimeSpan(15, 0, 0);
            var pmEndTime = new TimeSpan(19, 0, 0);

            var location = locationRepository.GetLatestVersionOfLocation(options.LocationIdentifier, options.Start);
            var approach = location.Approaches.Where(a => a.Id == options.ApproachId).FirstOrDefault();
            var detectorAggregations = detectorEventCountAggregationRepository.GetAggregationsBetweenDates(options.LocationIdentifier, options.Start, options.End).ToList();
            PeakHourResult result = new PeakHourResult();
            var peakResult = leftTurnReportService.GetAMPMPeakFlowRate(
                approach,
                options.Start,
                options.End,
                amStartTime,
                amEndTime,
                pmStartTime,
                pmEndTime,
                options.DaysOfWeek,
                detectorAggregations);
            var amPeak = peakResult.First();
            result.AmStartHour = amPeak.Key.Hours;
            result.AmStartMinute = amPeak.Key.Minutes;
            result.AmEndHour = amPeak.Key.Hours + 1;
            result.AmEndMinute = amPeak.Key.Minutes;

            var pmPeak = peakResult.Last();
            result.PmStartHour = pmPeak.Key.Hours;
            result.PmStartMinute = pmPeak.Key.Minutes;
            result.PmEndHour = pmPeak.Key.Hours + 1;
            result.PmEndMinute = pmPeak.Key.Minutes;

            return result;
        }
    }
}
