﻿using ATSPM.Application.Business.Common;
using ATSPM.Application.Business.PurdueCoordinationDiagram;
using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Application.TempExtensions;
using ATSPM.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ATSPM.Application.Business.LinkPivot
{
    public class LinkPivotPcdService
    {
        private readonly ILocationRepository locationRepository;
        private readonly LocationPhaseService locationPhaseService;
        private readonly IIndianaEventLogRepository controllerEventLogRepository;
        private readonly PurdueCoordinationDiagramService purdueCoordinationDiagramService;

        public LinkPivotPcdService(ILocationRepository locationRepository, IIndianaEventLogRepository controllerEventLogRepository, LocationPhaseService locationPhaseService, PurdueCoordinationDiagramService purdueCoordinationDiagramService)
        {
            this.locationRepository = locationRepository;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.locationPhaseService = locationPhaseService;
            this.purdueCoordinationDiagramService = purdueCoordinationDiagramService;
        }

        public async Task<LinkPivotPcdResult> GetData(LinkPivotPcdOptions options)
        {
            var result = new LinkPivotPcdResult();
            var startDate = options.StartDate.ToDateTime(options.StartTime);
            var endDate = options.EndDate.ToDateTime(options.EndTime);
            var upstreamLocation = locationRepository.GetLatestVersionOfLocation(options.LocationIdentifier);
            var downstreamLocation = locationRepository.GetLatestVersionOfLocation(options.DownLocationIdentifier);

            var upApproachToAnalyze = GetApproachToAnalyze(upstreamLocation, options.UpstreamDirection);
            var downApproachToAnalyze = GetApproachToAnalyze(downstreamLocation, options.DownDirection);

            if (upApproachToAnalyze != null)
                await GeneratePcdAsync(result, upApproachToAnalyze, options.Delta, startDate, endDate, true);
            if (downApproachToAnalyze != null)
                await GeneratePcdAsync(result, downApproachToAnalyze, options.Delta, startDate, endDate, false);

            return result;
        }

        private Approach GetApproachToAnalyze(Location location, string direction)
        {
            Approach approachToAnalyze = null;
            var approaches = location.Approaches.Where(a => a.DirectionType.Description == direction).ToList();
            foreach (var approach in approaches)
                if (approach.GetDetectorsForMetricType(6).Count > 0)
                    approachToAnalyze = approach;
            return approachToAnalyze;
        }

        private async Task GeneratePcdAsync(LinkPivotPcdResult result, Approach approach, int delta, DateTime startDate, DateTime endDate, bool upstream)
        {
            var chartName = string.Empty;
            //find the upstream approach
            if (!string.IsNullOrEmpty(approach.DirectionType.Description))
            {
                var logs = controllerEventLogRepository.GetEventsBetweenDates(approach.Location.LocationIdentifier, startDate, endDate).ToList();
                var plans = logs.GetPlanEvents(startDate, endDate).ToList();
                var lp = await locationPhaseService.GetLocationPhaseDataWithApproach(approach, startDate, endDate, 15, 13, logs, plans, true, null, 90);
                var pcdOptions = new PurdueCoordinationDiagramOptions()
                {
                    BinSize = 15,
                    GetVolume = true,
                    ShowPlanStatistics = true,
                    Start = startDate,
                    End = endDate,
                    locationIdentifier = approach.Location.LocationIdentifier
                };

                //Check the direction of the Link Pivot
                if (upstream)
                {
                    //Create a chart for the upstream detector before adjustments
                    result.pcdExisting.Add(purdueCoordinationDiagramService.GetChartData(pcdOptions, approach, lp));

                    //Add the total arrival on green before adjustments to the running total
                    result.ExistingTotalAOG += lp.TotalArrivalOnGreen;

                    //Add the volume from the signal phase to the running total
                    result.ExistingVolume += lp.TotalVolume;

                    //Re run the signal phase by the optimized delta change to get the adjusted pcd
                    locationPhaseService.LinkPivotAddSeconds(lp, delta * -1);

                    //Create a chart for the upstream detector after adjustments
                    pcdOptions.GetVolume = false;
                    result.pcdPredicted.Add(purdueCoordinationDiagramService.GetChartData(pcdOptions, approach, lp));

                    //Add the total arrival on green after adjustments to the running total
                    result.PredictedTotalAOG += lp.TotalArrivalOnGreen;

                    //Add the volume from the signal phase to the running total
                    result.PredictedVolume += lp.TotalVolume;
                }
                else
                {
                    //Create a chart for downstream detector before adjustments
                    result.pcdExisting.Add(purdueCoordinationDiagramService.GetChartData(pcdOptions, approach, lp));

                    //Add the arrivals on green to the total arrivals on green running total
                    result.ExistingTotalAOG += lp.TotalArrivalOnGreen;

                    //Add the volume before adjustments to the running total volume
                    result.ExistingVolume += lp.TotalVolume;

                    //Re run the signal phase by the optimized delta change to get the adjusted pcd
                    locationPhaseService.LinkPivotAddSeconds(lp, delta);

                    //Create a pcd chart for downstream after adjustments
                    pcdOptions.GetVolume = false;
                    result.pcdPredicted.Add(purdueCoordinationDiagramService.GetChartData(pcdOptions, approach, lp));

                    //Add the total arrivals on green to the running total
                    result.PredictedTotalAOG += lp.TotalArrivalOnGreen;

                    //Add the total volume to the running total after adjustments
                    result.PredictedVolume += lp.TotalVolume;
                }
            }
        }
    }
}