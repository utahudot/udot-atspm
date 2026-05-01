#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.LinkPivot/LinkPivotService.cs
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

using Microsoft.EntityFrameworkCore;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Extensions;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;
using Utah.Udot.Atspm.TempExtensions;

namespace Utah.Udot.Atspm.Business.LinkPivot
{
    public class LinkPivotService
    {
        private readonly ILocationRepository locationRepository;
        private readonly LinkPivotPairService linkPivotPairService;
        private readonly IIndianaEventLogRepository controllerEventLogRepository;

        public LinkPivotService(IIndianaEventLogRepository controllerEventLogRepository, ILocationRepository locationRepository, LinkPivotPairService linkPivotPairService)
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.locationRepository = locationRepository;
            this.linkPivotPairService = linkPivotPairService;
        }

        public async Task<LinkPivotResult> GetData(LinkPivotOptions options, List<RouteLocation> routeLocations)
        {
            routeLocations = routeLocations.OrderBy(routeLocation => routeLocation.Order).ToList();
            LinkPivot linkPivot = new LinkPivot(options.StartDate.ToDateTime(options.StartTime), options.EndDate.ToDateTime(options.EndTime));
            var (lp, pairedApproches) = await GetAdjustmentObjectsAsync(options, routeLocations);

            if (lp.Count == 0 || pairedApproches.Count == 0)
            {
                throw new Exception("Issue grabbing approach data for route locations.");
            }


            linkPivot.Adjustments = lp;
            linkPivot.PairedApproaches = pairedApproches;

            LinkPivotResult linkPivotResult = new LinkPivotResult();
            double totalVolume = 0;
            double totalDownstreamVolume = 0;
            double totalUpstreamVolume = 0;

            foreach (var a in linkPivot.Adjustments)
            {
                linkPivotResult.Adjustments.Add(new LinkPivotAdjustment(a.LinkNumber,
                    a.LocationIdentifier,
                    a.Location,
                    a.Delta,
                    a.ExistingOffset,
                    a.Adjustment));

                linkPivotResult.ApproachLinks.Add(new LinkPivotApproachLink(a.LocationIdentifier,
                    a.Location, a.UpstreamApproachDirection,
                    a.DownstreamLocationIdentifier, a.DownstreamLocation, a.DownstreamApproachDirection, a.PAOGUpstreamBefore,
                    a.PAOGUpstreamPredicted, a.PAOGDownstreamBefore, a.PAOGDownstreamPredicted,
                    a.AOGUpstreamBefore, a.AOGUpstreamPredicted, a.AOGDownstreamBefore,
                    a.AOGDownstreamPredicted, a.Delta, a.ResultChartLocation, a.AogTotalBefore,
                    a.PAogTotalBefore, a.AogTotalPredicted, a.PAogTotalPredicted, a.LinkNumber
                    ));

                totalVolume = totalVolume + a.DownstreamVolume + a.UpstreamVolume;
                totalDownstreamVolume = totalDownstreamVolume + a.DownstreamVolume;
                totalUpstreamVolume = totalUpstreamVolume + a.UpstreamVolume;
            }

            //Remove the last row from approch links because it will always be 0
            linkPivotResult.ApproachLinks.RemoveAt(linkPivotResult.ApproachLinks.Count - 1);

            //Get the totals
            linkPivotResult.TotalAogDownstreamBefore = linkPivot.Adjustments.Sum(a => a.AOGDownstreamBefore);
            linkPivotResult.TotalPaogDownstreamBefore = totalDownstreamVolume.AreEqual(0d) ? 0 : (int)Math.Round((linkPivot.Adjustments.Sum(a => a.AOGDownstreamBefore) / totalDownstreamVolume) * 100);
            if (double.IsNaN(linkPivotResult.TotalPaogDownstreamBefore))
            {
                // If result is NaN, set it to 0
                linkPivotResult.TotalPaogDownstreamBefore = 0;
            }
            linkPivotResult.TotalAogDownstreamPredicted = linkPivot.Adjustments.Sum(a => a.AOGDownstreamPredicted);
            linkPivotResult.TotalPaogDownstreamPredicted = totalDownstreamVolume.AreEqual(0d) ? 0 : (int)Math.Round((linkPivot.Adjustments.Sum(a => a.AOGDownstreamPredicted) / totalDownstreamVolume) * 100);
            if (double.IsNaN(linkPivotResult.TotalPaogDownstreamPredicted))
            {
                // If result is NaN, set it to 0
                linkPivotResult.TotalPaogDownstreamPredicted = 0;
            }

            linkPivotResult.TotalAogUpstreamBefore = linkPivot.Adjustments.Sum(a => a.AOGUpstreamBefore);
            linkPivotResult.TotalPaogUpstreamBefore = totalUpstreamVolume.AreEqual(0d) ? 0 : (int)Math.Round((linkPivot.Adjustments.Sum(a => a.AOGUpstreamBefore) / totalUpstreamVolume) * 100);
            if (double.IsNaN(linkPivotResult.TotalPaogUpstreamBefore))
            {
                // If result is NaN, set it to 0
                linkPivotResult.TotalPaogUpstreamBefore = 0;
            }
            linkPivotResult.TotalAogUpstreamPredicted = linkPivot.Adjustments.Sum(a => a.AOGUpstreamPredicted);
            linkPivotResult.TotalPaogUpstreamPredicted = totalUpstreamVolume.AreEqual(0d) ? 0 : (int)Math.Round((linkPivot.Adjustments.Sum(a => a.AOGUpstreamPredicted) / totalUpstreamVolume) * 100);
            if (double.IsNaN(linkPivotResult.TotalPaogUpstreamPredicted))
            {
                // If result is NaN, set it to 0
                linkPivotResult.TotalPaogUpstreamPredicted = 0;
            }

            linkPivotResult.TotalAogBefore = linkPivotResult.TotalAogUpstreamBefore + linkPivotResult.TotalAogDownstreamBefore;
            linkPivotResult.TotalPaogBefore = totalVolume.AreEqual(0d) ? 0 : (int)Math.Round((linkPivotResult.TotalAogBefore / totalVolume) * 100);
            if (double.IsNaN(linkPivotResult.TotalPaogBefore))
            {
                // If result is NaN, set it to 0
                linkPivotResult.TotalPaogBefore = 0;
            }

            linkPivotResult.TotalAogPredicted = linkPivotResult.TotalAogUpstreamPredicted + linkPivotResult.TotalAogDownstreamPredicted;
            linkPivotResult.TotalPaogPredicted = totalVolume.AreEqual(0d) ? 0 : (int)Math.Round((linkPivotResult.TotalAogPredicted / totalVolume) * 100);
            if (double.IsNaN(linkPivotResult.TotalPaogPredicted))
            {
                // If result is NaN, set it to 0
                linkPivotResult.TotalPaogPredicted = 0;
            }

            linkPivotResult.SetSummary();

            return linkPivotResult;
        }

        private async Task<(List<AdjustmentObject>, List<LinkPivotPair>)> GetAdjustmentObjectsAsync(LinkPivotOptions options, List<RouteLocation> routeLocations)
        {
            List<LinkPivotPair> pairedApproaches = new List<LinkPivotPair>();
            List<AdjustmentObject> adjustments = new List<AdjustmentObject>();
            var routeLocationsInTraversalOrder = GetRouteLocationsInTraversalOrder(options, routeLocations);
            var existingOffsetsByLocationIdentifier = routeLocationsInTraversalOrder
                .Select(routeLocation => routeLocation.LocationIdentifier)
                .Distinct()
                .ToDictionary(
                    locationIdentifier => locationIdentifier,
                    locationIdentifier => GetExistingOffset(options, locationIdentifier));

            int[] daysOfWeek = options.DaysOfWeek ?? Array.Empty<int>();
            var daysToInclude = GetDaysToProcess(options.StartDate, options.EndDate, daysOfWeek);
            await CreatePairedApproaches(
                routeLocationsInTraversalOrder,
                pairedApproaches,
                daysToInclude,
                options);

            //Cycle through the LinkPivotPair list and add the statistics to the LinkPivotadjustmentTable
            for (var i = 0; i < routeLocationsInTraversalOrder.Count - 1; i++)
            {
                var routeLocation = routeLocationsInTraversalOrder[i];

                //Make sure the list is in the correct order after parrallel processing
                var lpp = pairedApproaches.FirstOrDefault(p =>
                    p.UpstreamLocationApproach.Location.LocationIdentifier == routeLocation.LocationIdentifier);
                if (lpp != null)
                {
                    var a = new AdjustmentObject()
                    {
                        LocationIdentifier = lpp.UpstreamLocationApproach.Location.LocationIdentifier,
                        Location = FormatLocationName(lpp.UpstreamLocationApproach.Location),
                        DownstreamLocation = FormatLocationName(lpp.DownstreamLocationApproach.Location),
                        Delta = Convert.ToInt32(lpp.SecondsAdded),
                        ExistingOffset = GetExistingOffsetValue(
                            existingOffsetsByLocationIdentifier,
                            lpp.UpstreamLocationApproach.Location.LocationIdentifier),
                        PAOGDownstreamBefore = lpp.PaogDownstreamBefore,
                        PAOGDownstreamPredicted = lpp.PaogDownstreamPredicted,
                        PAOGUpstreamBefore = lpp.PaogUpstreamBefore,
                        PAOGUpstreamPredicted = lpp.PaogUpstreamPredicted,
                        AOGDownstreamBefore = lpp.AogDownstreamBefore,
                        AOGDownstreamPredicted = lpp.AogDownstreamPredicted,
                        AOGUpstreamBefore = lpp.AogUpstreamBefore,
                        AOGUpstreamPredicted = lpp.AogUpstreamPredicted,
                        DownstreamLocationIdentifier = lpp.DownstreamLocationApproach.Location.LocationIdentifier,
                        DownstreamApproachDirection = lpp.DownstreamLocationApproach.DirectionType.Description,
                        UpstreamApproachDirection = lpp.UpstreamLocationApproach.DirectionType.Description,
                        ResultChartLocation = lpp.ResultChartLocation,
                        AogTotalBefore = lpp.AogTotalBefore,
                        PAogTotalBefore = lpp.PaogTotalBefore,
                        AogTotalPredicted = lpp.AogTotalPredicted,
                        PAogTotalPredicted = lpp.PaogTotalPredicted,
                        LinkNumber = lpp.LinkNumber,
                        DownstreamVolume = lpp.TotalVolumeDownstream,
                        UpstreamVolume = lpp.TotalVolumeUpstream
                    };
                    adjustments.Add(a);
                }
            }

            //Set the end row to have zero for the ajustments. No adjustment can be made because 
            //downstream is unknown. The end row is determined by the starting point seleceted by the user
            AddLastAdjusment(
                routeLocationsInTraversalOrder.LastOrDefault(),
                adjustments,
                existingOffsetsByLocationIdentifier);

            var cumulativeChange = 0;

            //Determine the adjustment by adding the previous rows adjustment to the current rows delta
            for (var i = adjustments.Count - 1; i >= 0; i--)
            {
                //if the new adjustment is greater than the cycle time than the adjustment should subtract
                // the cycle time from the current adjustment and the result should be the new adjustment
                if (cumulativeChange + adjustments[i].Delta > options.CycleLength)
                {
                    adjustments[i].Adjustment = cumulativeChange + adjustments[i].Delta - options.CycleLength;
                    cumulativeChange = cumulativeChange + adjustments[i].Delta - options.CycleLength;
                }
                else
                {
                    adjustments[i].Adjustment = cumulativeChange + adjustments[i].Delta;
                    cumulativeChange = cumulativeChange + adjustments[i].Delta;
                }
            }
            return (adjustments, pairedApproaches);
        }

        private static List<RouteLocation> GetRouteLocationsInTraversalOrder(LinkPivotOptions options, List<RouteLocation> routeLocations)
        {
            return options.Direction == "Upstream"
                ? routeLocations.OrderByDescending(routeLocation => routeLocation.Order).ToList()
                : routeLocations.OrderBy(routeLocation => routeLocation.Order).ToList();
        }

        private void AddLastAdjusment(
            RouteLocation routeLocation,
            List<AdjustmentObject> adjustments,
            Dictionary<string, int> existingOffsetsByLocationIdentifier)
        {
            if (routeLocation != null)
            {
                var location = locationRepository.GetLatestVersionOfLocation(routeLocation.LocationIdentifier);

                adjustments.Add(new AdjustmentObject()
                {
                    LocationIdentifier = routeLocation.LocationIdentifier,
                    Location = FormatLocationName(location),
                    DownstreamLocation = "",
                    Delta = 0,
                    ExistingOffset = GetExistingOffsetValue(
                        existingOffsetsByLocationIdentifier,
                        routeLocation.LocationIdentifier),
                    PAOGDownstreamBefore = 0,
                    PAOGDownstreamPredicted = 0,
                    PAOGUpstreamBefore = 0,
                    PAOGUpstreamPredicted = 0,
                    AOGDownstreamBefore = 0,
                    AOGDownstreamPredicted = 0,
                    AOGUpstreamBefore = 0,
                    AOGUpstreamPredicted = 0,
                    DownstreamLocationIdentifier = routeLocation.LocationIdentifier,
                    DownstreamApproachDirection = routeLocation.PrimaryDirection != null ? routeLocation.PrimaryDirection.Description : "",
                    UpstreamApproachDirection = routeLocation.PrimaryDirection != null ? routeLocation.PrimaryDirection?.Description : "",
                    ResultChartLocation = "",
                    AogTotalBefore = 0,
                    PAogTotalBefore = 0,
                    AogTotalPredicted = 0,
                    PAogTotalPredicted = 0,
                    LinkNumber = adjustments.Count + 1,
                    DownstreamVolume = 0,
                    UpstreamVolume = 0
                });
            }
        }

        private static string FormatLocationName(Location location)
        {
            if (location == null)
            {
                return "";
            }

            return string.Join(" & ", new[] { location.PrimaryName, location.SecondaryName }
                .Where(name => !string.IsNullOrWhiteSpace(name)));
        }

        private int GetExistingOffset(LinkPivotOptions options, string locationIdentifier)
        {
            var start = options.StartDate.ToDateTime(options.StartTime);
            var end = options.EndDate.ToDateTime(options.EndTime);
            var controllerEventLogs = controllerEventLogRepository
                .GetEventsBetweenDates(locationIdentifier, start.AddHours(-12), end.AddHours(12))
                .ToList();
            var offsetLengthChangeEvents = controllerEventLogs
                .GetEventsByEventCodes(
                    start.AddHours(-12),
                    end.AddHours(12),
                    new List<short> { (short)IndianaEnumerations.OffsetLengthChange });

            return GetEventOverlappingTime(start, offsetLengthChangeEvents)
                .FirstOrDefault()?.EventParam ?? 0;
        }

        private static int GetExistingOffsetValue(
            Dictionary<string, int> existingOffsetsByLocationIdentifier,
            string locationIdentifier)
        {
            return existingOffsetsByLocationIdentifier.TryGetValue(locationIdentifier, out var existingOffset)
                ? existingOffset
                : 0;
        }

        private static List<IndianaEvent> GetEventOverlappingTime(
            DateTime start,
            IReadOnlyList<IndianaEvent> events)
        {
            var eventAtStart = events.Where(e => e.Timestamp == start).ToList();
            if (eventAtStart.Count == 0)
            {
                var eventsInTimeSpan = events.Where(e => e.Timestamp < start)
                    ?.GroupBy(log => log.EventCode)
                    ?.Select(group => group.OrderByDescending(e => e.Timestamp).FirstOrDefault())
                    .ToList();

                if (eventsInTimeSpan != null && eventsInTimeSpan.Count != 0)
                    eventAtStart = eventsInTimeSpan;
            }

            return eventAtStart.ToList();
        }

        private async Task CreatePairedApproaches(
            List<RouteLocation> routeLocations,
            List<LinkPivotPair> PairedApproaches,
            List<DateOnly> daysToInclude,
            LinkPivotOptions options)
        {
            for (var i = 0; i < routeLocations.Count - 1; i++)
            {
                var upstreamRouteLocation = routeLocations[i];
                var downstreamRouteLocation = routeLocations[i + 1];

                var upstreamLocation = locationRepository.GetLatestVersionOfLocation(upstreamRouteLocation.LocationIdentifier);
                var downstreamLocation = locationRepository.GetLatestVersionOfLocation(downstreamRouteLocation.LocationIdentifier);

                var upstreamApproach = GetApproach(upstreamLocation, upstreamRouteLocation, downstreamRouteLocation);
                var downstreamApproach = GetApproach(downstreamLocation, downstreamRouteLocation, upstreamRouteLocation);
                var linkNumber = i + 1;

                var linkPivotPair = await linkPivotPairService.GetLinkPivotPairAsync(upstreamApproach, downstreamApproach, options, daysToInclude, linkNumber);
                PairedApproaches.Add(linkPivotPair);
            }
        }

        private static Approach GetApproach(Location location, RouteLocation routeLocation, RouteLocation adjacentRouteLocation)
        {
            var usePrimaryApproach = adjacentRouteLocation.Order > routeLocation.Order;
            var phaseNumber = usePrimaryApproach ? routeLocation.PrimaryPhase : routeLocation.OpposingPhase;
            var directionTypeId = usePrimaryApproach ? routeLocation.PrimaryDirectionId : routeLocation.OpposingDirectionId;
            var isOverlap = usePrimaryApproach ? routeLocation.IsPrimaryOverlap : routeLocation.IsOpposingOverlap;

            return location.Approaches.FirstOrDefault(a =>
                a.ProtectedPhaseNumber == phaseNumber &&
                a.DirectionTypeId == directionTypeId &&
                a.IsProtectedPhaseOverlap == isOverlap
                );
        }

        private List<DateOnly> GetDaysToProcess(DateOnly startDate, DateOnly endDate, int[] daysOfWeek)
        {
            bool addAllDays = daysOfWeek.Length == 0;
            List<DateOnly> datesToInclude = new List<DateOnly>();
            var days = endDate.DayNumber - startDate.DayNumber;

            for (int i = 0; i <= days; i++)
            {
                var date = startDate.AddDays(i);
                if (daysOfWeek.Contains((int)date.DayOfWeek) || addAllDays)
                {
                    datesToInclude.Add(date);
                }
            }

            return datesToInclude;
        }
    }
}
