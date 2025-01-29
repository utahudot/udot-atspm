﻿#region license
// Copyright 2024 Utah Departement of Transportation
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
using Utah.Udot.Atspm.Extensions;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

namespace Utah.Udot.Atspm.Business.LinkPivot
{
    public class LinkPivotService
    {
        private readonly ILocationRepository locationRepository;
        private readonly LinkPivotPairService linkPivotPairService;

        public LinkPivotService(ILocationRepository locationRepository, LinkPivotPairService linkPivotPairService)
        {
            this.locationRepository = locationRepository;
            this.linkPivotPairService = linkPivotPairService;
        }

        public async Task<LinkPivotResult> GetData(LinkPivotOptions options, List<RouteLocation> routeLocations)
        {
            LinkPivot linkPivot = new LinkPivot(options.StartDate.ToDateTime(options.StartTime), options.EndDate.ToDateTime(options.EndTime));
            var (lp, pairedApproches) = await GetAdjustmentObjectsAsync(options, routeLocations);

            if (lp.Count == 0 || pairedApproches.Count == 0) {
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
                    a.Location.ToString(),
                    a.Delta,
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
            var indices = new List<int>();

            if (options.Direction == "Upstream")
            {
                for (var i = routeLocations.Count - 1; i > 0; i--)
                {
                    indices.Add(i);
                }
            }
            else
            {
                for (var i = 0; i < routeLocations.Count - 1; i++)
                {
                    indices.Add(i);
                }
            }

            var daysToInclude = GetDaysToProcess(options.StartDate, options.EndDate, options.DaysOfWeek);
            await CreatePairedApproaches(options, routeLocations, pairedApproaches, indices, daysToInclude);

            //Cycle through the LinkPivotPair list and add the statistics to the LinkPivotadjustmentTable
            foreach (var i in indices)
            {
                //Make sure the list is in the correct order after parrallel processing
                var lpp = pairedApproaches.FirstOrDefault(p =>
                    p.UpstreamLocationApproach.Location.LocationIdentifier == routeLocations[i].LocationIdentifier);
                if (lpp != null)
                {
                    var a = new AdjustmentObject()
                    {
                        LocationIdentifier = lpp.UpstreamLocationApproach.Location.LocationIdentifier,
                        Location = lpp.UpstreamLocationApproach.Location.ToString(),
                        DownstreamLocation = lpp.DownstreamLocationApproach.Location.ToString(),
                        Delta = Convert.ToInt32(lpp.SecondsAdded),
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
            if (options.Direction == "Upstream")
            {
                AddLastAdjusment(routeLocations.FirstOrDefault(), adjustments);
            }
            else
            {
                AddLastAdjusment(routeLocations.LastOrDefault(), adjustments);
            }

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

        private static void AddLastAdjusment(RouteLocation routeLocation, List<AdjustmentObject> adjustments)
        {
            if(routeLocation != null)
            {
                adjustments.Add(new AdjustmentObject()
                {
                    LocationIdentifier = routeLocation.LocationIdentifier,
                    Location = routeLocation.ToString(),
                    DownstreamLocation = "",
                    Delta = 0,
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
                    LinkNumber = routeLocation.Order,
                    DownstreamVolume = 0,
                    UpstreamVolume = 0
                });
            }
        }

        private async Task CreatePairedApproaches(LinkPivotOptions options, List<RouteLocation> routeLocations, List<LinkPivotPair> PairedApproaches, List<int> indices, List<DateOnly> daysToInclude)
        {
            foreach (var i in indices)
            {
                var location = locationRepository.GetLatestVersionOfLocation(routeLocations[options.Direction == "Upstream" ? i - 1 : i].LocationIdentifier);
                var primaryPhase = routeLocations[i].PrimaryPhase;
                var downstreamPrimaryPhase = routeLocations[options.Direction == "Upstream" ? i - 1 : i + 1].OpposingPhase;
                if (downstreamPrimaryPhase != null)
                {
                    var downstreamLocation = locationRepository.GetLatestVersionOfLocation(routeLocations[options.Direction == "Upstream" ? i : i + 1].LocationIdentifier);
                    var downstreamApproach = getDownstreamApproach(location, downstreamLocation, primaryPhase, downstreamPrimaryPhase, options.Direction);
                    var approach = getApproach(location, downstreamLocation, primaryPhase, downstreamPrimaryPhase, options.Direction);
                    var linkPivotPair = await linkPivotPairService.GetLinkPivotPairAsync(approach, downstreamApproach, options, daysToInclude, i + 1);
                    PairedApproaches.Add(linkPivotPair);
                }
            }
        }

        private Approach getDownstreamApproach(Location location, Location downstreamLocation, int primaryPhase, int downstreamPrimaryPhase, string direction)
        {
            if (direction == "Upstream")
            {
                return location.Approaches.FirstOrDefault(a => a.ProtectedPhaseNumber == primaryPhase);
            }
            else
            {
                return downstreamLocation.Approaches.FirstOrDefault(a => a.ProtectedPhaseNumber == downstreamPrimaryPhase);
            }
        }

        private Approach getApproach(Location location, Location downstreamLocation, int primaryPhase, int downstreamPrimaryPhase, string direction)
        {
            if (direction == "Upstream")
            {
                return downstreamLocation.Approaches.FirstOrDefault(a => a.ProtectedPhaseNumber == downstreamPrimaryPhase);
            }
            else
            {
                return location.Approaches.FirstOrDefault(a => a.ProtectedPhaseNumber == primaryPhase);
            }
        }

        private List<DateOnly> GetDaysToProcess(DateOnly startDate, DateOnly endDate, int[] daysOfWeek)
        {
            List<DateOnly> datesToInclude = new List<DateOnly>();
            var days = endDate.DayNumber - startDate.DayNumber;

            for (int i = 0; i <= days; i++)
            {
                var date = startDate.AddDays(i);
                if (daysOfWeek.Contains((int)date.DayOfWeek))
                {
                    datesToInclude.Add(date);
                }
            }

            return datesToInclude;
        }
    }
}
