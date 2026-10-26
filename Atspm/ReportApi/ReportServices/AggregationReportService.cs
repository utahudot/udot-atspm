#region license
// Copyright 2026 Utah Departement of Transportation
// for ReportApi - Utah.Udot.Atspm.ReportApi.ReportServices/AggregationReportService.cs
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

using Utah.Udot.Atspm.ReportApi.DataAggregation;

namespace Utah.Udot.Atspm.ReportApi.ReportServices
{


    /// <summary>
    /// Approach delay report service
    /// </summary>
    public class AggregationReportService : ReportServiceBase<AggregationOptions, IEnumerable<AggregationResult>>
    {
        private readonly ILocationRepository locationRepository;
        private readonly DetectorVolumeAggregationOptions detectorVolumeAggregationOptions;
        private readonly ApproachSpeedAggregationOptions approachSpeedAggregationOptions;
        private readonly ApproachPcdAggregationOptions approachPcdAggregationOptions;
        private readonly PhaseCycleAggregationOptions phaseCycleAggregationOptions;
        private readonly ApproachSplitFailAggregationOptions approachSplitFailAggregationOptions;
        private readonly ApproachYellowRedActivationsAggregationOptions approachYellowRedActivationsAggregationOptions;
        private readonly PreemptionAggregationOptions preemptionAggregationOptions;
        private readonly PriorityAggregationOptions priorityAggregationOptions;
        private readonly SignalEventCountAggregationOptions signalEventCountAggregationOptions;
        private readonly PhaseTerminationAggregationOptions phaseTerminationAggregationOptions;
        private readonly PhasePedAggregationOptions phasePedAggregationOptions;
        private readonly PhaseLeftTurnGapAggregationOptions phaseLeftTurnGapAggregationOptions;
        private readonly PhaseSplitMonitorAggregationOptions phaseSplitMonitorAggregationOptions;
        private readonly IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository;
        private readonly ILogger<AggregationReportService> logger;

        /// <inheritdoc/>
        public AggregationReportService(
            ILocationRepository locationRepository,
            DetectorVolumeAggregationOptions detectorVolumeAggregationOptions,
            ApproachSpeedAggregationOptions approachSpeedAggregationOptions,
            ApproachPcdAggregationOptions approachPcdAggregationOptions,
            PhaseCycleAggregationOptions phaseCycleAggregationOptions,
            ApproachSplitFailAggregationOptions approachSplitFailAggregationOptions,
            ApproachYellowRedActivationsAggregationOptions approachYellowRedActivationsAggregationOptions,
            PreemptionAggregationOptions preemptionAggregationOptions,
            PriorityAggregationOptions priorityAggregationOptions,
            SignalEventCountAggregationOptions signalEventCountAggregationOptions,
            PhaseTerminationAggregationOptions phaseTerminationAggregationOptions,
            PhasePedAggregationOptions phasePedAggregationOptions,
            PhaseLeftTurnGapAggregationOptions phaseLeftTurnGapAggregationOptions,
            PhaseSplitMonitorAggregationOptions phaseSplitMonitorAggregationOptions,
            IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository,
            ILogger<AggregationReportService> logger
            )
        {
            this.locationRepository = locationRepository;
            this.detectorVolumeAggregationOptions = detectorVolumeAggregationOptions;
            this.approachSpeedAggregationOptions = approachSpeedAggregationOptions;
            this.approachPcdAggregationOptions = approachPcdAggregationOptions;
            this.phaseCycleAggregationOptions = phaseCycleAggregationOptions;
            this.approachSplitFailAggregationOptions = approachSplitFailAggregationOptions;
            this.approachYellowRedActivationsAggregationOptions = approachYellowRedActivationsAggregationOptions;
            this.preemptionAggregationOptions = preemptionAggregationOptions;
            this.priorityAggregationOptions = priorityAggregationOptions;
            this.signalEventCountAggregationOptions = signalEventCountAggregationOptions;
            this.phaseTerminationAggregationOptions = phaseTerminationAggregationOptions;
            this.phasePedAggregationOptions = phasePedAggregationOptions;
            this.phaseLeftTurnGapAggregationOptions = phaseLeftTurnGapAggregationOptions;
            this.phaseSplitMonitorAggregationOptions = phaseSplitMonitorAggregationOptions;
            this.detectorEventCountAggregationRepository = detectorEventCountAggregationRepository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<AggregationResult>> ExecuteAsync(AggregationOptions options, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            // Checked as the charts will use them, so a Time Of Day request over the whole
            // range is checked with its chosen days.
            SignalAggregationMetricOptions.SetTimeOfDayWindow(options);
            var optionsError = GetOptionsError(options);
            if (optionsError != null)
                throw new ArgumentException(optionsError);

            switch (options.AggregationType)
            {
                case AggregationType.DetectorEventCount:
                    return detectorVolumeAggregationOptions.CreateMetric(options);
                case AggregationType.Speed:
                    return approachSpeedAggregationOptions.CreateMetric(options);
                case AggregationType.Pcd:
                    return approachPcdAggregationOptions.CreateMetric(options);
                case AggregationType.PhaseCycle:
                    return phaseCycleAggregationOptions.CreateMetric(options);
                case AggregationType.SplitFail:
                    return approachSplitFailAggregationOptions.CreateMetric(options);
                case AggregationType.YellowRedActivation:
                    return approachYellowRedActivationsAggregationOptions.CreateMetric(options);
                case AggregationType.Preemption:
                    return preemptionAggregationOptions.CreateMetric(options);
                case AggregationType.Priority:
                    return priorityAggregationOptions.CreateMetric(options);
                case AggregationType.SignalEventCount:
                    return signalEventCountAggregationOptions.CreateMetric(options);
                case AggregationType.PhaseTermination:
                    return phaseTerminationAggregationOptions.CreateMetric(options);
                case AggregationType.Ped:
                    return phasePedAggregationOptions.CreateMetric(options);
                case AggregationType.PhaseLeftTurn:
                    return phaseLeftTurnGapAggregationOptions.CreateMetric(options);
                case AggregationType.SplitMonitor:
                    return phaseSplitMonitorAggregationOptions.CreateMetric(options);
                default:
                    throw new Exception("Unknown Chart Type");
            }

        }

        // The report is open to any caller, so options the page would never send are
        // turned away with a reason instead of charting nothing as zeros.
        internal static string GetOptionsError(AggregationOptions options)
        {
            if (options.TimeOptions == null)
                return "Time options are required.";
            if (options.End <= options.Start)
                return "End must be later than Start.";
            // Data is fetched between Start and End but charted in the time options' slots.
            if (options.Start != options.TimeOptions.Start || options.End != options.TimeOptions.End)
                return "Start and End must match the time options' Start and End.";
            // Only the location list is charted; the list of location numbers is not read.
            if (options.Locations == null || !options.Locations.Any() || options.Locations.Contains(null))
                return "Select at least one location.";
            if (options.Locations.All(location => location.Exclude))
                return "All of the selected locations are excluded.";
            return options.TimeOptions.GetValidationError();
        }

        //private ActionResult GetLaneByLaneChart(AggregationOptions aggDataExportViewModel)
        //{
        //    return GetChart(aggDataExportViewModel, options);
        //}

        //private void SetLocations(AggregationOptions options, List<Location> locations)
        //{
        //    foreach (var LocationIdentifier in options.LocationIdentifiers)
        //    {
        //        var location = locationRepository.GetLatestVersionOfLocation(LocationIdentifier, options.Start);
        //        if (location != null)
        //        {
        //            locations.Add(location);
        //        }
        //    }
        //}
    }
}
