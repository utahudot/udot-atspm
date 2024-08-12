﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - ATSPM.ReportApi.ReportServices/LeftTurnGapReportService.cs
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


using Utah.Udot.Atspm.Business.LeftTurnGapReport;

namespace Utah.Udot.Atspm.ReportApi.ReportServices
{
    /// <summary>
    /// Left turn gap analysis report service
    /// </summary>
    public class LeftTurnGapReportService : ReportServiceBase<LeftTurnGapReportOptions, IEnumerable<LeftTurnGapReportResult>>
    {
        private readonly IApproachRepository approachRepository;
        private readonly ILocationRepository locationRepository;
        private readonly IPhaseCycleAggregationRepository phaseCycleAggregationRepository;
        private readonly IPhaseTerminationAggregationRepository phaseTerminationAggregationRepository;
        private readonly IPhasePedAggregationRepository phasePedAggregationRepository;
        private readonly IApproachSplitFailAggregationRepository approachSplitFailAggregationRepository;
        private readonly IPhaseLeftTurnGapAggregationRepository phaseLeftTurnGapAggregationRepository;
        private readonly IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository;
        private readonly LeftTurnReportService leftTurnReportService;
        private readonly LeftTurnPeakHourService leftTurnPeakHourService;
        private readonly LeftTurnGapDurationService leftTurnGapDurationService;
        private readonly LeftTurnSplitFailService leftTurnSplitFailService;
        private readonly LeftTurnPedActuationService leftTurnPedActuationService;
        private readonly LeftTurnVolumeService leftTurnVolumeService;
        private readonly ILogger<LeftTurnGapReportDataCheckService> logger;

        /// <inheritdoc/>
        public LeftTurnGapReportService(
            IApproachRepository approachRepository,
            ILocationRepository locationRepository,
            IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository,
            IPhaseCycleAggregationRepository phaseCycleAggregationRepository,
            IPhaseTerminationAggregationRepository phaseTerminationAggregationRepository,
            IPhasePedAggregationRepository phasePedAggregationRepository,
            IApproachSplitFailAggregationRepository approachSplitFailAggregationRepository,
            IPhaseLeftTurnGapAggregationRepository phaseLeftTurnGapAggregationRepository,
            LeftTurnReportService leftTurnReportPreCheckService,
            LeftTurnPeakHourService leftTurnPeakHourService,
            LeftTurnGapDurationService leftTurnGapDurationService,
            LeftTurnSplitFailService leftTurnSplitFailService,
            LeftTurnPedActuationService leftTurnPedActuationService,
            LeftTurnVolumeService leftTurnVolumeService,
            ILogger<LeftTurnGapReportDataCheckService> logger)
        {
            this.approachRepository = approachRepository;
            this.locationRepository = locationRepository;
            this.phaseCycleAggregationRepository = phaseCycleAggregationRepository;
            this.phaseTerminationAggregationRepository = phaseTerminationAggregationRepository;
            this.phasePedAggregationRepository = phasePedAggregationRepository;
            this.approachSplitFailAggregationRepository = approachSplitFailAggregationRepository;
            this.phaseLeftTurnGapAggregationRepository = phaseLeftTurnGapAggregationRepository;
            this.detectorEventCountAggregationRepository = detectorEventCountAggregationRepository;
            leftTurnReportService = leftTurnReportPreCheckService;
            this.leftTurnPeakHourService = leftTurnPeakHourService;
            this.leftTurnGapDurationService = leftTurnGapDurationService;
            this.leftTurnSplitFailService = leftTurnSplitFailService;
            this.leftTurnPedActuationService = leftTurnPedActuationService;
            this.leftTurnVolumeService = leftTurnVolumeService;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<LeftTurnGapReportResult>> ExecuteAsync(LeftTurnGapReportOptions options, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var results = new List<LeftTurnGapReportResult>();
            var location = locationRepository.GetLatestVersionOfLocation(options.LocationIdentifier, options.Start);
            foreach (int approachId in options.ApproachIds)
            {
                var approach = location.Approaches.Where(a => a.Id == approachId).FirstOrDefault();
                if (approach == null)
                {
                    continue;
                }
                if (options.GetAMPMPeakPeriod)
                {
                    SetHoursAndMinutes(options, 6, 0, 9, 0);
                    var approachResultAM = await GetApproachResult(options, approach, approachId);
                    approachResultAM.PeakPeriodDescription = "AM Peak";
                    results.Add(approachResultAM);

                    SetHoursAndMinutes(options, 15, 0, 18, 0);
                    var approachResultPM = await GetApproachResult(options, approach, approachId);
                    approachResultPM.PeakPeriodDescription = "PM Peak";
                    results.Add(approachResultPM);
                }
                else if (options.GetAMPMPeakHour)
                {
                    var peakHourOptions = new PeakHourOptions
                    {
                        LocationIdentifier = options.LocationIdentifier,
                        ApproachId = approachId,
                        DaysOfWeek = options.DaysOfWeek,
                        Start = options.Start,
                        End = options.End

                    };
                    var peakResult = await leftTurnPeakHourService.ExecuteAsync(peakHourOptions, null);

                    SetHoursAndMinutes(options, peakResult.AmStartHour, peakResult.AmStartMinute, peakResult.AmEndHour, peakResult.AmEndMinute);
                    var approachResultAM = await GetApproachResult(options, approach, approachId);
                    approachResultAM.PeakPeriodDescription = "AM Peak";
                    results.Add(approachResultAM);


                    SetHoursAndMinutes(options, peakResult.PmStartHour, peakResult.PmStartMinute, peakResult.PmEndHour, peakResult.PmEndMinute);
                    var approachResultPM = await GetApproachResult(options, approach, approachId);
                    approachResultPM.PeakPeriodDescription = "PM Peak";
                    results.Add(approachResultPM);
                }
                else if (options.Get24HourPeriod)
                {
                    var approachResult = await GetApproachResult(options, approach, approachId);
                    approachResult.Get24HourPeriod = true;
                    results.Add(approachResult);
                }
                else
                {
                    var approachResult = await GetApproachResult(options, approach, approachId);
                    approachResult.PeakPeriodDescription = "Custom";
                    results.Add(approachResult);
                }
            }
            return results;
        }



        private static void SetHoursAndMinutes(LeftTurnGapReportOptions options, int startHour, int startMinute, int endHour, int endMinute)
        {
            options.StartHour = startHour;
            options.StartMinute = startMinute;
            options.EndHour = endHour;
            options.EndMinute = endMinute;
        }

        private async Task<LeftTurnGapReportResult> GetApproachResult(LeftTurnGapReportOptions options, Approach approach, int approachId)
        {
            LeftTurnGapReportResult approachResult = new LeftTurnGapReportResult
            {
                SignalId = options.LocationIdentifier,
                StartDate = options.Start,
                EndDate = options.End,
                StartTime = new TimeSpan(options.StartHour ?? 0, options.StartMinute ?? 0, 0),
                EndTime = new TimeSpan(options.EndHour ?? 0, options.EndMinute ?? 0, 0),
                ApproachDescription = approach.Description,
                SpeedLimit = approach.Mph,
                Location = approach.Location.PrimaryName + " & " + approach.Location.SecondaryName,
                PhaseType = approach.GetPhaseType().GetDisplayName(),
                SignalType = approach.GetSignalHeadType().GetDisplayName()
            };

            if (options.GetGapReport)
            {
                var leftTurnGapOptions = new GapDurationOptions
                {
                    ApproachId = approach.Id,
                    DaysOfWeek = options.DaysOfWeek,
                    Start = options.Start,
                    End = options.End,
                    StartHour = options.StartHour == null ? 0 : options.StartHour.Value,
                    StartMinute = options.StartMinute == null ? 0 : options.StartMinute.Value,
                    EndHour = options.EndHour == null ? 23 : options.EndHour.Value,
                    EndMinute = options.EndMinute == null ? 59 : options.EndMinute.Value,
                    LocationIdentifier = approach.Location.LocationIdentifier
                };
                var gapResult = await leftTurnGapDurationService.ExecuteAsync(leftTurnGapOptions, null);
                approachResult.GapDurationConsiderForStudy = gapResult.GapDurationPercent > options.AcceptableGapPercentage;
                approachResult.Capacity = gapResult.Capacity;
                approachResult.Demand = gapResult.Demand;
                approachResult.VCRatio = gapResult.Capacity == 0 ? 0 : gapResult.Demand / gapResult.Capacity;
                approachResult.GapOutPercent = gapResult.GapDurationPercent;
                approachResult.AcceptableGapList = gapResult.AcceptableGaps;
            }
            if (options.GetSplitFail)
            {
                var leftTurnSplitFailOptions = new LeftTurnSplitFailOptions
                {
                    ApproachId = approach.Id,
                    DaysOfWeek = options.DaysOfWeek,
                    Start = options.Start,
                    End = options.End,
                    StartHour = options.StartHour == null ? 0 : options.StartHour.Value,
                    StartMinute = options.StartMinute == null ? 0 : options.StartMinute.Value,
                    EndHour = options.EndHour == null ? 23 : options.EndHour.Value,
                    EndMinute = options.EndMinute == null ? 59 : options.EndMinute.Value,
                    LocationIdentifier = approach.Location.LocationIdentifier
                };

                var splitFailResult = await leftTurnSplitFailService.ExecuteAsync(leftTurnSplitFailOptions, null);

                approachResult.SplitFailsConsiderForStudy = splitFailResult.SplitFailPercent > options.AcceptableSplitFailPercentage;
                approachResult.CyclesWithSplitFailNum = splitFailResult.CyclesWithSplitFails;
                approachResult.CyclesWithSplitFailPercent = splitFailResult.SplitFailPercent;
                approachResult.PercentCyclesWithSplitFailList = splitFailResult.PercentCyclesWithSplitFailList;
                approachResult.Direction = splitFailResult.Direction;
            }
            if (options.GetPedestrianCall)
            {
                var pedActuationOptions = new PedActuationOptions
                {
                    ApproachId = approach.Id,
                    DaysOfWeek = options.DaysOfWeek,
                    Start = options.Start,
                    End = options.End,
                    StartHour = options.StartHour == null ? 0 : options.StartHour.Value,
                    StartMinute = options.StartMinute == null ? 0 : options.StartMinute.Value,
                    EndHour = options.EndHour == null ? 23 : options.EndHour.Value,
                    EndMinute = options.EndMinute == null ? 59 : options.EndMinute.Value,
                    LocationIdentifier = approach.Location.LocationIdentifier
                };

                var PedResult = await leftTurnPedActuationService.ExecuteAsync(pedActuationOptions, null);

                approachResult.CyclesWithPedCallNum = PedResult.CyclesWithPedCallsNum;
                approachResult.CyclesWithPedCallPercent = PedResult.CyclesWithPedCallsPercent;
                approachResult.PedActuationsConsiderForStudy = PedResult.CyclesWithPedCallsPercent > 0.3d;
                approachResult.PercentCyclesWithPedsList = PedResult.PercentCyclesWithPedsList;
                approachResult.Direction = PedResult.Direction;
                approachResult.OpposingDirection = PedResult.OpposingDirection;
            }
            if (options.GetConflictingVolume || options.GetGapReport)
            {
                var volumeOptions = new VolumeOptions
                {
                    ApproachId = approach.Id,
                    DaysOfWeek = options.DaysOfWeek,
                    Start = options.Start,
                    End = options.End,
                    StartHour = options.StartHour == null ? 0 : options.StartHour.Value,
                    StartMinute = options.StartMinute == null ? 0 : options.StartMinute.Value,
                    EndHour = options.EndHour == null ? 23 : options.EndHour.Value,
                    EndMinute = options.EndMinute == null ? 59 : options.EndMinute.Value,
                    LocationIdentifier = approach.Location.LocationIdentifier
                };
                var volumeResult = await leftTurnVolumeService.ExecuteAsync(volumeOptions, null);

                if (options.GetConflictingVolume)
                {
                    var considerForStudy = volumeResult.CrossProductReview || volumeResult.DecisionBoundariesReview;
                    approachResult.CrossProductConsiderForStudy = considerForStudy;
                    approachResult.VolumesConsiderForStudy = considerForStudy;
                }
                approachResult.OpposingLanes = volumeResult.OpposingLanes;
                approachResult.CrossProductReview = volumeResult.CrossProductReview;
                approachResult.DecisionBoundariesReview = volumeResult.DecisionBoundariesReview;
                approachResult.LeftTurnVolume = volumeResult.LeftTurnVolume;
                approachResult.OpposingThroughVolume = volumeResult.OpposingThroughVolume;
                approachResult.CrossProductValue = volumeResult.CrossProductValue;
                approachResult.CalculatedVolumeBoundary = volumeResult.CalculatedVolumeBoundary;
                approachResult.DemandList = volumeResult.DemandList;
                approachResult.Direction = volumeResult.Direction;
                approachResult.OpposingDirection = volumeResult.OpposingDirection;
            }
            return approachResult;
        }

    }
}
