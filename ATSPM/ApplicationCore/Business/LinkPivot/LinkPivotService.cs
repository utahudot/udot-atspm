using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.LinkPivot
{
    public class LinkPivotService
    {
        private readonly IRouteRepository routeRepository;
        private readonly ILocationRepository locationRepository;

        public LinkPivotService(IRouteRepository routeRepository, ILocationRepository locationRepository)
        {
            this.routeRepository = routeRepository;
            this.locationRepository = locationRepository;
        }

        protected LinkPivotResult GetData(LinkPivotOptions options)
        {
            List<DayOfWeek> daysList = new List<DayOfWeek>();
            if (options.Monday)
            {
                daysList.Add(DayOfWeek.Monday);
            }
            if (options.Tuesday)
            {
                daysList.Add(DayOfWeek.Tuesday);
            }
            if (options.Wednesday)
            {
                daysList.Add(DayOfWeek.Wednesday);
            }
            if (options.Thursday)
            {
                daysList.Add(DayOfWeek.Thursday);
            }
            if (options.Friday)
            {
                daysList.Add(DayOfWeek.Friday);
            }
            if (options.Saturday)
            {
                daysList.Add(DayOfWeek.Saturday);
            }
            if (options.Sunday)
            {
                daysList.Add(DayOfWeek.Sunday);
            }

            //Generate a Link Pivot Object
            var lp = GetLinkPivot(options, daysList);

            //Instantiate the return object
            List<AdjustmentObject> adjustments = new List<AdjustmentObject>();

            //Add the data from the Link Pivot Object to the return object
            foreach (MOE.Common.Data.LinkPivot.LinkPivotAdjustmentRow row in lp.Adjustment)
            {
                AdjustmentObject a = new AdjustmentObject();
                a.SignalId = row.SignalId;
                a.Location = row.Location;
                a.DownstreamLocation = row.DownstreamLocation;
                a.Delta = row.Delta;
                a.Adjustment = row.Adjustment;
                a.PAOGDownstreamBefore = row.PAOGDownstreamBefore;
                a.PAOGDownstreamPredicted = row.PAOGDownstreamPredicted;
                a.PAOGUpstreamBefore = row.PAOGUpstreamBefore;
                a.PAOGUpstreamPredicted = row.PAOGUpstreamPredicted;
                a.AOGDownstreamBefore = row.AOGDownstreamBefore;
                a.AOGDownstreamPredicted = row.AOGDownstreamPredicted;
                a.AOGUpstreamBefore = row.AOGUpstreamBefore;
                a.AOGUpstreamPredicted = row.AOGUpstreamPredicted;
                a.DownSignalId = row.DownstreamSignalID;
                a.DownstreamApproachDirection = row.DownstreamApproachDirection;
                a.UpstreamApproachDirection = row.UpstreamApproachDirection;
                a.ResultChartLocation = row.ResultChartLocation;
                a.AogTotalBefore = row.AOGTotalBefore;
                a.PAogTotalBefore = row.PAOGToatalBefore;
                a.AogTotalPredicted = row.AOGTotalPredicted;
                a.PAogTotalPredicted = row.PAOGTotalPredicted;
                a.LinkNumber = row.LinkNumber;
                a.DownstreamVolume = row.DownstreamVolume;
                a.UpstreamVolume = row.UpstreamVolume;
                adjustments.Add(a);
            }

            return adjustments.ToArray();
        }

        public LinkPivotResult GetLinkPivot(LinkPivotOptions options, List<DayOfWeek> days)
        {
            var route = routeRepository.GetList().Include(r => r.RouteLocations).Where(r => r.Id == options.RouteId).FirstOrDefault();
            //var signalList = new Dictionary<string, Location>();
            //foreach (var location in route.RouteLocations)
            //{
            //    signalList.Add(location.LocationIdentifier, locationRepository.GetVersionOfLocationByDate(location.LocationIdentifier, options.Start));
            //}
            var routeLocations = route.RouteLocations.OrderBy(r => r.Order).ToList();
            // Get a list of dates that matches the parameters passed by the user
            var dates = GetDates(options.Start, options.End, days);
            //Make a list of numbers to use as indices to perform parallelism 
            var indices = new List<int>();
            if (options.Direction == "Upstream")
            {
                for (var i = route.RouteLocations.Count - 1; i > 0; i--)
                    indices.Add(i);
                //Parallel.ForEach(indices, i =>
                foreach (var i in indices)
                {
                    var previousLocation = routeLocations[i - 1];
                    var signal = locationRepository.GetLatestVersionOfLocation(routeLocations[i - 1].LocationIdentifier, options.Start);
                    //var primaryRouteLocation =
                    //    routeLocations[i].PhaseDirections.FirstOrDefault(p => p.IsPrimaryApproach);
                    //var opposingRouteLocation =
                    //    route.RouteSignals[i - 1].PhaseDirections.FirstOrDefault(p => p.IsPrimaryApproach == false);

                    //TODO: Not sure if I should use OpposingDirection or If I should use OpposingDirectionID
                    if (previousLocation.OpposingDirection != null)
                    {
                        var downstreamSignal =
                            signalRepository.GetVersionOfSignalByDate(route.RouteSignals[i].SignalId, startDate);
                        var downstreamApproach = signal.Approaches.FirstOrDefault(a =>
                            a.DirectionTypeID == primaryPhaseDirection.DirectionTypeId &&
                            a.IsProtectedPhaseOverlap == primaryPhaseDirection.IsOverlap &&
                            a.ProtectedPhaseNumber == primaryPhaseDirection.Phase);
                        var approach = downstreamSignal.Approaches.FirstOrDefault(a =>
                            a.DirectionTypeID == downstreamPrimaryPhaseDirection.DirectionTypeId &&
                            a.IsProtectedPhaseOverlap == downstreamPrimaryPhaseDirection.IsOverlap &&
                            a.ProtectedPhaseNumber == downstreamPrimaryPhaseDirection.Phase);
                        PairedApproaches.Add(new LinkPivotPair(approach, downstreamApproach, startDate, endDate,
                            cycleTime,
                            bias, biasDirection, dates, i + 1));
                    }
                }
                //);
            }
            else
            {
                for (var i = 0; i < routeLocations.Count - 1; i++)
                    indices.Add(i);
                //Parallel.ForEach(indices, i =>
                foreach (var i in indices)
                {
                    var signal = locationRepository.GetLatestVersionOfLocation(routeLocations[i].LocationIdentifier, options.Start);
                    var primaryRouteLocation = routeLocations[i];
                    var opposingRouteLocation = routeLocations[i + 1];
                    if (opposingRouteLocation != null)
                    {
                        var downstreamSignal = locationRepository.GetLatestVersionOfLocation(routeLocations[i + 1].LocationIdentifier, options.Start);
                        var approach = signal.Approaches.FirstOrDefault(a =>
                            a.DirectionTypeId == primaryRouteLocation.PrimaryDirectionId &&
                            a.IsProtectedPhaseOverlap == primaryRouteLocation.IsPrimaryOverlap &&
                            a.ProtectedPhaseNumber == primaryRouteLocation.PrimaryPhase);
                        var downstreamApproach = downstreamSignal.Approaches.FirstOrDefault(a =>
                            a.DirectionTypeId == opposingRouteLocation.OpposingDirectionId &&
                            a.IsProtectedPhaseOverlap == opposingRouteLocation.IsOpposingOverlap &&
                            a.ProtectedPhaseNumber == opposingRouteLocation.OpposingPhase);
                        PairedApproaches.Add(new LinkPivotPair(approach, downstreamApproach, startDate, endDate,
                            cycleTime,
                            bias, biasDirection, dates, i + 1));
                    }
                }
                //);
            }

            if (Adjustment != null)
            {
                //Cycle through the LinkPivotPair list and add the statistics to the LinkPivotadjustmentTable
                foreach (var i in indices)
                {
                    //Make sure the list is in the correct order after parrallel processing
                    var lpp = PairedApproaches.FirstOrDefault(p =>
                        p.SignalApproach.SignalID == route.RouteSignals[i].SignalId);
                    if (lpp != null)
                        Adjustment.AddLinkPivotAdjustmentRow(
                            lpp.SignalApproach.SignalID,
                            Convert.ToInt32(lpp.SecondsAdded),
                            0,
                            lpp.PaogUpstreamBefore,
                            lpp.PaogDownstreamBefore,
                            lpp.AogUpstreamBefore,
                            lpp.AogDownstreamBefore,
                            lpp.PaogUpstreamPredicted,
                            lpp.PaogDownstreamPredicted,
                            lpp.AogUpstreamPredicted,
                            lpp.AogDownstreamPredicted,
                            lpp.SignalApproach.Signal.SignalDescription,
                            lpp.DownSignalApproach.Signal.SignalID,
                            lpp.DownSignalApproach.DirectionType.Description,
                            lpp.SignalApproach.DirectionType.Description,
                            lpp.ResultChartLocation,
                            lpp.DownSignalApproach.Signal.SignalDescription,
                            lpp.AogTotalBefore,
                            lpp.PaogTotalBefore,
                            lpp.AogTotalPredicted,
                            lpp.PaogTotalPredicted,
                            lpp.LinkNumber,
                            lpp.TotalVolumeDownstream,
                            lpp.TotalVolumeUpstream);
                }

                //Set the end row to have zero for the ajustments. No adjustment can be made because 
                //downstream is unknown. The end row is determined by the starting point seleceted by the user
                if (direction == "Upstream")
                    Adjustment.AddLinkPivotAdjustmentRow(route.RouteSignals.FirstOrDefault().SignalId, 0, 0, 0, 0, 0, 0,
                        0,
                        0, 0, 0,
                        route.RouteSignals.FirstOrDefault().Signal.SignalDescription,
                        "", "", "", "", "", 0, 0, 0, 0, 1, 0, 0);
                else
                    Adjustment.AddLinkPivotAdjustmentRow(
                        route.RouteSignals.LastOrDefault().SignalId, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, route.RouteSignals.LastOrDefault().Signal.SignalDescription, "", "",
                        "",
                        "", "", 0, 0, 0, 0,
                        route.RouteSignals.Count, 0, 0);

                var cumulativeChange = 0;

                //Determine the adjustment by adding the previous rows adjustment to the current rows delta
                for (var i = Adjustment.Count - 1; i >= 0; i--)
                    //if the new adjustment is greater than the cycle time than the adjustment should subtract
                    // the cycle time from the current adjustment and the result should be the new adjustment
                    if (cumulativeChange + Adjustment[i].Delta > cycleTime)
                    {
                        Adjustment[i].Adjustment = cumulativeChange + Adjustment[i].Delta - cycleTime;
                        cumulativeChange = cumulativeChange + Adjustment[i].Delta - cycleTime;
                    }
                    else
                    {
                        Adjustment[i].Adjustment = cumulativeChange + Adjustment[i].Delta;
                        cumulativeChange = cumulativeChange + Adjustment[i].Delta;
                    }
            }
        }

        private static List<DateTime> GetDates(DateTime startDate, DateTime endDate, List<DayOfWeek> days)
        {
            //Find each day in the given period that matches one of the specified day types and add it to the return list
            var dates = new List<DateTime>();
            for (var dt = startDate; dt <= endDate; dt = dt.AddDays(1))
                if (days.Contains(dt.DayOfWeek))
                    dates.Add(dt);
            return dates;
        }
    }
}
