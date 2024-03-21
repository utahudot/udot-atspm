using ATSPM.Application.Business;
using ATSPM.Application.Business.Aggregation;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.ReportApi.DataAggregation;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.ReportServices
{


    /// <summary>
    /// Approach delay report service
    /// </summary>
    public class AggregationReportService : ReportServiceBase<AggregationOptions, IEnumerable<AggregationResult>>
    {
        private readonly ILocationRepository locationRepository;

        /// <inheritdoc/>
        public AggregationReportService(
            ILocationRepository locationRepository
            )
        {
            this.locationRepository = locationRepository;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<AggregationResult>> ExecuteAsync(AggregationOptions options, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            switch (options.AggregationType)
            {
                case 16:
                    return GetLaneByLaneChart(options);
                //case 25:
                //    return GetApproachSpeedAggregationChart(aggDataExportViewModel);
                //case 18:
                //    return GetPCDChart(aggDataExportViewModel);
                //case 19:
                //    return GetCycleChart(aggDataExportViewModel);
                //case 20:
                //    return GetSplitFailChart(aggDataExportViewModel);
                //case 26:
                //    return GetYraChart(aggDataExportViewModel);
                //case 22:
                //    return GetPreemptionChart(aggDataExportViewModel);
                //case 24:
                //    return GetPriorityChart(aggDataExportViewModel);
                //case 27:
                //    return GetSignalEventCountChart(aggDataExportViewModel);
                //case 29:
                //    return GetPhaseTerminationChart(aggDataExportViewModel);
                //case 30:
                //    return GetPhasePedChart(aggDataExportViewModel);
                //case 34:
                //    return GetLeftTurnGapChart(aggDataExportViewModel);
                //case 35:
                //    return GetSplitMonitorChart(aggDataExportViewModel);
                default:
                    throw new Exception("Unknown Chart Type");
            }

        }

        private ActionResult GetLaneByLaneChart(AggregationOptions aggDataExportViewModel)
        {
            return GetChart(aggDataExportViewModel, options);
        }

        //private void SetLocations(AggregationOptions options, List<Location> locations)
        //{
        //    foreach (var locationIdentifier in options.LocationIdentifiers)
        //    {
        //        var location = locationRepository.GetLatestVersionOfLocation(locationIdentifier, options.Start);
        //        if (location != null)
        //        {
        //            locations.Add(location);
        //        }
        //    }
        //}
    }
}
