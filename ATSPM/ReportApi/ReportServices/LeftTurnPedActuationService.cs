#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - ATSPM.ReportApi.ReportServices/LeftTurnPedActuationService.cs
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
    public class LeftTurnPedActuationService : ReportServiceBase<PedActuationOptions, PedActuationResult>
    {
        private readonly ILocationRepository locationRepository;
        private readonly PedActuationService pedActuationService;
        private readonly IPhasePedAggregationRepository phasePedAggregationRepository;
        private readonly IPhaseCycleAggregationRepository phaseCycleAggregationRepository;
        private readonly LeftTurnReportService leftTurnReportService;
        private readonly ILogger<LeftTurnPedActuationService> logger;

        /// <inheritdoc/>
        public LeftTurnPedActuationService(
            ILocationRepository locationRepository,
            PedActuationService pedActuationService,
            IPhasePedAggregationRepository phasePedAggregationRepository,
            IPhaseCycleAggregationRepository phaseCycleAggregationRepository,
            LeftTurnReportService leftTurnReportService,
            ILogger<LeftTurnPedActuationService> logger)
        {
            this.locationRepository = locationRepository;
            this.pedActuationService = pedActuationService;
            this.phasePedAggregationRepository = phasePedAggregationRepository;
            this.phaseCycleAggregationRepository = phaseCycleAggregationRepository;
            this.leftTurnReportService = leftTurnReportService;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public override async Task<PedActuationResult> ExecuteAsync(PedActuationOptions options, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var location = locationRepository.GetLatestVersionOfLocation(options.LocationIdentifier, options.Start);
            var approach = location.Approaches.Where(a => a.Id == options.ApproachId).FirstOrDefault();
            var startTime = new TimeSpan(options.StartHour, options.StartMinute, 0);
            var endTime = new TimeSpan(options.EndHour, options.EndMinute, 0);
            var pedActuationResult = new PedActuationResult();
            var pedAggregations = phasePedAggregationRepository.GetAggregationsBetweenDates(options.LocationIdentifier, options.Start, options.End).ToList();
            var cycelAggregations = phaseCycleAggregationRepository.GetAggregationsBetweenDates(options.LocationIdentifier, options.Start, options.End).ToList();
            var opposingPhase = leftTurnReportService.GetOpposingPhase(approach);
            pedActuationResult = pedActuationService.GetPedestrianPercentage(location, approach, options, startTime, endTime, pedAggregations, opposingPhase, cycelAggregations);
            return pedActuationResult;
        }
    }
}
