#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - ATSPM.ReportApi.ReportServices/LeftTurnSplitFailService.cs
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
using ATSPM.Data.Models;
using ATSPM.Data.Models.AggregationModels;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Left turn gap analysis report service
    /// </summary>
    public class LeftTurnSplitFailService : ReportServiceBase<LeftTurnSplitFailOptions, LeftTurnSplitFailResult>
    {
        private readonly ILocationRepository locationRepository;
        private readonly IApproachSplitFailAggregationRepository approachSplitFailAggregationRepository;
        private readonly SplitFailService splitFailService;
        private readonly ILogger<LeftTurnSplitFailService> logger;

        /// <inheritdoc/>
        public LeftTurnSplitFailService(
            ILocationRepository locationRepository,
            IApproachSplitFailAggregationRepository approachSplitFailAggregationRepository,
            SplitFailService splitFailService,
            ILogger<LeftTurnSplitFailService> logger)
        {
            this.locationRepository = locationRepository;
            this.approachSplitFailAggregationRepository = approachSplitFailAggregationRepository;
            this.splitFailService = splitFailService;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public override async Task<LeftTurnSplitFailResult> ExecuteAsync(LeftTurnSplitFailOptions options, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var location = locationRepository.GetLatestVersionOfLocation(options.LocationIdentifier, options.Start);
            var approach = location.Approaches.Where(a => a.Id == options.ApproachId).FirstOrDefault();
            var splitFailResult = new LeftTurnSplitFailResult();
            var splitfailaggregations = GetSplitFailAggregates(options, approach);
            splitFailResult = splitFailService.GetSplitFailPercent(options, splitfailaggregations);
            return splitFailResult;
        }

        private List<ApproachSplitFailAggregation> GetSplitFailAggregates(LeftTurnSplitFailOptions options, Approach approach)
        {

            var startTime = new TimeSpan(options.StartHour, options.StartMinute, 0);
            var endTime = new TimeSpan(options.EndHour, options.EndMinute, 0);
            List<ApproachSplitFailAggregation> splitFailsAggregates = new List<ApproachSplitFailAggregation>();
            for (var tempDate = options.Start.Date; tempDate <= options.End; tempDate = tempDate.AddDays(1))
            {
                if (options.DaysOfWeek.Contains((int)options.Start.DayOfWeek))
                {
                    //HACK: had to change this, but it needs location identifier. I put null in here for now
                    splitFailsAggregates.AddRange(approachSplitFailAggregationRepository
                        .GetAggregationsBetweenDates(options.LocationIdentifier, tempDate.Date.Add(startTime), tempDate.Date.Add(endTime))
                        .Where(a => a.ApproachId == approach.Id));
                }
            }

            return splitFailsAggregates;
        }


    }
}
