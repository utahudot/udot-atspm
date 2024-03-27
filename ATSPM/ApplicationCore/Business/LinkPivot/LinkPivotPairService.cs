using ATSPM.Application.Business.Common;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Application.TempExtensions;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATSPM.Application.Business.LinkPivot
{
    public class LinkPivotPairService
    {
        private readonly LocationPhaseService locationPhaseService;
        private readonly IIndianaEventLogRepository controllerEventLogRepository;

        public LinkPivotPairService(LocationPhaseService locationPhaseService, IIndianaEventLogRepository controllerEventLogRepository)
        {
            this.locationPhaseService = locationPhaseService;
            this.controllerEventLogRepository = controllerEventLogRepository;
        }

        public async Task<LinkPivotPair> GetLinkPivotPairAsync(Approach signalApproach,
            Approach downSignalApproach,
            LinkPivotOptions options,
            List<DateOnly> daysToInclude,
            int linkNumber)
        {
            LinkPivotPair linkPivotPair = new LinkPivotPair();

            linkPivotPair.UpstreamLocationApproach = signalApproach;
            linkPivotPair.DownstreamLocationApproach = downSignalApproach;
            linkPivotPair.StartDate = options.StartDate;
            linkPivotPair.LinkNumber = linkNumber;
            await SetPcds(options.StartTime, options.EndTime, daysToInclude, options.CycleLength, linkPivotPair);
            //Check to see if both directions have detection if so analyze both
            if (linkPivotPair.UpstreamPcd.Count > 0 && linkPivotPair.DownstreamPcd.Count > 0)
                if (options.Bias != 0)
                    GetBiasedLinkPivot(options.CycleLength, options.Bias, options.BiasDirection, daysToInclude, linkPivotPair);
                //If no bias is provided
                else
                    GetUnbiasedLinkPivot(options.CycleLength, daysToInclude, linkPivotPair);
            //If only upstream has detection do analysis for upstream only
            else if (linkPivotPair.DownstreamPcd.Count == 0 && linkPivotPair.UpstreamPcd.Count > 0)
                if (options.Bias != 0)
                {
                    double upstreamBias = 1;
                    double downstreamBias = 1;
                    if (options.BiasDirection == "Downstream")
                        downstreamBias = 1 + options.Bias / 100;
                    else
                        upstreamBias = 1 + options.Bias / 100;
                    //set the original values to compare against
                    var maxBiasArrivalOnGreen = linkPivotPair.AogDownstreamBefore * downstreamBias +
                                                linkPivotPair.AogUpstreamBefore * upstreamBias;
                    linkPivotPair.MaxArrivalOnGreen = linkPivotPair.AogUpstreamBefore;
                    //Add the total to the results grid
                    linkPivotPair.ResultsGraph.Add(0, linkPivotPair.MaxArrivalOnGreen);
                    linkPivotPair.UpstreamResultsGraph.Add(0, linkPivotPair.AogUpstreamBefore * upstreamBias);
                    linkPivotPair.DownstreamResultsGraph.Add(0, linkPivotPair.AogDownstreamBefore * downstreamBias);
                    linkPivotPair.AogUpstreamPredicted = linkPivotPair.AogUpstreamBefore;
                    linkPivotPair.PaogUpstreamPredicted = Math.Round(linkPivotPair.AogUpstreamBefore / linkPivotPair.TotalVolumeUpstream, 2) * 100;
                    linkPivotPair.SecondsAdded = 0;

                    for (var i = 1; i <= options.CycleLength; i++)
                    {
                        double totalBiasArrivalOnGreen = 0;
                        double totalArrivalOnGreen = 0;
                        double totalUpstreamAog = 0;


                        for (var index = 0; index < daysToInclude.Count; index++)
                        {
                            locationPhaseService.LinkPivotAddSeconds(linkPivotPair.UpstreamPcd[index], -1);
                            //UpstreamPcd[index].LinkPivotAddSeconds(-1);

                            totalBiasArrivalOnGreen += linkPivotPair.UpstreamPcd[index].TotalArrivalOnGreen * upstreamBias;
                            totalArrivalOnGreen += linkPivotPair.UpstreamPcd[index].TotalArrivalOnGreen;
                            totalUpstreamAog += linkPivotPair.UpstreamPcd[index].TotalArrivalOnGreen;
                        }
                        //Add the total aog to the dictionary
                        linkPivotPair.ResultsGraph.Add(i, totalBiasArrivalOnGreen);
                        linkPivotPair.UpstreamResultsGraph.Add(i, totalUpstreamAog);

                        if (totalBiasArrivalOnGreen > maxBiasArrivalOnGreen)
                        {
                            maxBiasArrivalOnGreen = totalBiasArrivalOnGreen;
                            linkPivotPair.MaxArrivalOnGreen = totalArrivalOnGreen;
                            linkPivotPair.AogUpstreamPredicted = totalUpstreamAog;
                            linkPivotPair.PaogUpstreamPredicted = Math.Round(totalUpstreamAog / linkPivotPair.TotalVolumeUpstream, 2) * 100;
                            linkPivotPair.SecondsAdded = i;
                        }
                    }
                    //Get the link totals
                    linkPivotPair.AogTotalPredicted = linkPivotPair.MaxArrivalOnGreen;
                    linkPivotPair.PaogTotalPredicted = linkPivotPair.PaogUpstreamPredicted;
                }
                //No bias provided
                else
                {
                    //set the original values to compare against
                    linkPivotPair.AogTotalBefore = linkPivotPair.AogDownstreamBefore + linkPivotPair.AogUpstreamBefore;
                    linkPivotPair.MaxArrivalOnGreen = linkPivotPair.AogUpstreamBefore;
                    linkPivotPair.PaogTotalBefore = Math.Round(linkPivotPair.AogTotalBefore / (linkPivotPair.TotalVolumeDownstream + linkPivotPair.TotalVolumeUpstream), 2) *
                                      100;


                    //Add the total aog to the dictionary
                    linkPivotPair.ResultsGraph.Add(0, linkPivotPair.MaxArrivalOnGreen);
                    linkPivotPair.UpstreamResultsGraph.Add(0, linkPivotPair.AogUpstreamBefore);
                    linkPivotPair.DownstreamResultsGraph.Add(0, linkPivotPair.AogDownstreamBefore);
                    linkPivotPair.AogUpstreamPredicted = linkPivotPair.AogUpstreamBefore;
                    linkPivotPair.PaogUpstreamPredicted = Math.Round(linkPivotPair.AogUpstreamBefore / linkPivotPair.TotalVolumeUpstream, 2) * 100;
                    linkPivotPair.SecondsAdded = 0;
                    for (var i = 1; i <= options.CycleLength; i++)
                    {
                        double totalArrivalOnGreen = 0;
                        double totalUpstreamAog = 0;

                        for (var index = 0; index < daysToInclude.Count; index++)
                        {
                            locationPhaseService.LinkPivotAddSeconds(linkPivotPair.UpstreamPcd[index], -1);
                            //UpstreamPcd[index].LinkPivotAddSeconds(-1);
                            totalArrivalOnGreen += linkPivotPair.UpstreamPcd[index].TotalArrivalOnGreen;
                            totalUpstreamAog += linkPivotPair.UpstreamPcd[index].TotalArrivalOnGreen;
                        }
                        //Add the total aog to the dictionary
                        linkPivotPair.ResultsGraph.Add(i, totalArrivalOnGreen);
                        linkPivotPair.UpstreamResultsGraph.Add(i, totalUpstreamAog);

                        if (totalArrivalOnGreen > linkPivotPair.MaxArrivalOnGreen)
                        {
                            linkPivotPair.MaxArrivalOnGreen = totalArrivalOnGreen;
                            linkPivotPair.AogUpstreamPredicted = totalUpstreamAog;
                            linkPivotPair.PaogUpstreamPredicted = Math.Round(totalUpstreamAog / linkPivotPair.TotalVolumeUpstream, 2) * 100;
                            linkPivotPair.SecondsAdded = i;
                        }
                        //Get the link totals
                        linkPivotPair.AogTotalPredicted = linkPivotPair.MaxArrivalOnGreen;
                        linkPivotPair.PaogTotalPredicted = linkPivotPair.PaogUpstreamPredicted;
                    }
                }
            //If downsteam only has detection
            else if (linkPivotPair.UpstreamPcd.Count == 0 && linkPivotPair.DownstreamPcd.Count > 0)
                if (options.Bias != 0)
                {
                    double upstreamBias = 1;
                    double downstreamBias = 1;
                    if (options.BiasDirection == "Downstream")
                        downstreamBias = 1 + options.Bias / 100;
                    else
                        upstreamBias = 1 + options.Bias / 100;
                    //set the original values to compare against
                    var maxBiasArrivalOnGreen = linkPivotPair.AogDownstreamBefore * downstreamBias;
                    linkPivotPair.MaxArrivalOnGreen = linkPivotPair.AogDownstreamBefore + linkPivotPair.AogUpstreamBefore;
                    //Add the total aog to the dictionary
                    linkPivotPair.ResultsGraph.Add(0, linkPivotPair.MaxArrivalOnGreen);
                    linkPivotPair.DownstreamResultsGraph.Add(0, linkPivotPair.AogDownstreamBefore * downstreamBias);
                    linkPivotPair.AogDownstreamPredicted = linkPivotPair.AogDownstreamBefore;
                    linkPivotPair.PaogDownstreamPredicted = Math.Round(linkPivotPair.AogDownstreamBefore / linkPivotPair.TotalVolumeDownstream, 2) * 100;
                    linkPivotPair.SecondsAdded = 0;

                    for (var i = 1; i <= options.CycleLength; i++)
                    {
                        double totalBiasArrivalOnGreen = 0;
                        double totalArrivalOnGreen = 0;
                        double totalDownstreamAog = 0;

                        for (var index = 0; index < daysToInclude.Count; index++)
                        {
                            locationPhaseService.LinkPivotAddSeconds(linkPivotPair.DownstreamPcd[index], 1);
                            totalBiasArrivalOnGreen += linkPivotPair.DownstreamPcd[index].TotalArrivalOnGreen * downstreamBias;
                            totalArrivalOnGreen += linkPivotPair.DownstreamPcd[index].TotalArrivalOnGreen;
                            totalDownstreamAog += linkPivotPair.DownstreamPcd[index].TotalArrivalOnGreen;
                        }
                        //Add the total aog to the dictionary
                        linkPivotPair.ResultsGraph.Add(i, totalBiasArrivalOnGreen);
                        linkPivotPair.DownstreamResultsGraph.Add(i, totalDownstreamAog);
                        if (totalBiasArrivalOnGreen > maxBiasArrivalOnGreen)
                        {
                            maxBiasArrivalOnGreen = totalBiasArrivalOnGreen;
                            linkPivotPair.MaxArrivalOnGreen = totalArrivalOnGreen;
                            linkPivotPair.AogDownstreamPredicted = totalDownstreamAog;
                            linkPivotPair.PaogDownstreamPredicted = Math.Round(totalDownstreamAog / linkPivotPair.TotalVolumeDownstream, 2) * 100;
                            linkPivotPair.SecondsAdded = i;
                        }
                    }
                    //Get the link totals
                    linkPivotPair.AogTotalPredicted = linkPivotPair.MaxArrivalOnGreen;
                    linkPivotPair.PaogTotalPredicted = linkPivotPair.PaogDownstreamPredicted;
                }
                //if no bias was provided
                else
                {
                    //set the original values to compare against
                    linkPivotPair.MaxArrivalOnGreen = linkPivotPair.AogDownstreamBefore;
                    //Add the total aog to the dictionary
                    linkPivotPair.ResultsGraph.Add(0, linkPivotPair.MaxArrivalOnGreen);
                    linkPivotPair.DownstreamResultsGraph.Add(0, linkPivotPair.AogDownstreamBefore);
                    linkPivotPair.AogDownstreamPredicted = linkPivotPair.AogDownstreamBefore;
                    linkPivotPair.PaogDownstreamPredicted = Math.Round(linkPivotPair.AogDownstreamBefore / linkPivotPair.TotalVolumeDownstream, 2) * 100;
                    linkPivotPair.SecondsAdded = 0;

                    for (var i = 1; i <= options.CycleLength; i++)
                    {
                        double totalArrivalOnGreen = 0;
                        double totalDownstreamAog = 0;

                        for (var index = 0; index < daysToInclude.Count; index++)
                        {
                            locationPhaseService.LinkPivotAddSeconds(linkPivotPair.DownstreamPcd[index], 1);
                            totalArrivalOnGreen += linkPivotPair.DownstreamPcd[index].TotalArrivalOnGreen;
                            totalDownstreamAog += linkPivotPair.DownstreamPcd[index].TotalArrivalOnGreen;
                        }
                        //Add the total aog to the dictionary
                        linkPivotPair.ResultsGraph.Add(i, totalArrivalOnGreen);
                        linkPivotPair.DownstreamResultsGraph.Add(i, totalDownstreamAog);
                        if (totalArrivalOnGreen > linkPivotPair.MaxArrivalOnGreen)
                        {
                            linkPivotPair.MaxArrivalOnGreen = totalArrivalOnGreen;
                            linkPivotPair.AogDownstreamPredicted = totalDownstreamAog;
                            linkPivotPair.PaogDownstreamPredicted = Math.Round(totalDownstreamAog / linkPivotPair.TotalVolumeDownstream, 2) * 100;
                            linkPivotPair.SecondsAdded = i;
                        }
                    }
                    //Get the link totals
                    linkPivotPair.AogTotalPredicted = linkPivotPair.MaxArrivalOnGreen;
                    linkPivotPair.PaogTotalPredicted = linkPivotPair.PaogDownstreamPredicted;
                }

            return linkPivotPair;
        }

        private void GetUnbiasedLinkPivot(int cycleTime, List<DateOnly> dates, LinkPivotPair linkPivotPair)
        {
            //set the original values to compare against
            linkPivotPair.AogTotalBefore = linkPivotPair.AogDownstreamBefore + linkPivotPair.AogUpstreamBefore;
            linkPivotPair.MaxArrivalOnGreen = linkPivotPair.AogTotalBefore;
            linkPivotPair.PaogTotalBefore = Math.Round(linkPivotPair.AogTotalBefore / (linkPivotPair.TotalVolumeDownstream + linkPivotPair.TotalVolumeUpstream), 2) * 100;
            //add the total to the results grid
            linkPivotPair.ResultsGraph.Add(0, linkPivotPair.MaxArrivalOnGreen);
            linkPivotPair.UpstreamResultsGraph.Add(0, linkPivotPair.AogUpstreamBefore);
            linkPivotPair.DownstreamResultsGraph.Add(0, linkPivotPair.AogDownstreamBefore);

            linkPivotPair.AogUpstreamPredicted = linkPivotPair.AogUpstreamBefore;
            linkPivotPair.AogDownstreamPredicted = linkPivotPair.AogDownstreamBefore;
            linkPivotPair.PaogDownstreamPredicted = Math.Round(linkPivotPair.AogDownstreamBefore / linkPivotPair.TotalVolumeDownstream, 2) * 100;
            linkPivotPair.PaogUpstreamPredicted = Math.Round(linkPivotPair.AogUpstreamBefore / linkPivotPair.TotalVolumeUpstream, 2) * 100;
            linkPivotPair.SecondsAdded = 0;

            for (var i = 1; i <= cycleTime; i++)
            {
                double totalArrivalOnGreen = 0;
                double totalUpstreamAog = 0;
                double totalDownstreamAog = 0;

                for (var index = 0; index < dates.Count; index++)
                {
                    locationPhaseService.LinkPivotAddSeconds(linkPivotPair.UpstreamPcd[index], -1);
                    locationPhaseService.LinkPivotAddSeconds(linkPivotPair.DownstreamPcd[index], 1);
                    totalArrivalOnGreen += linkPivotPair.UpstreamPcd[index].TotalArrivalOnGreen +
                                           linkPivotPair.DownstreamPcd[index].TotalArrivalOnGreen;
                    totalUpstreamAog += linkPivotPair.UpstreamPcd[index].TotalArrivalOnGreen;
                    totalDownstreamAog += linkPivotPair.DownstreamPcd[index].TotalArrivalOnGreen;
                }
                //Add the total aog to the dictionary
                linkPivotPair.ResultsGraph.Add(i, totalArrivalOnGreen);
                linkPivotPair.UpstreamResultsGraph.Add(i, totalUpstreamAog);
                linkPivotPair.DownstreamResultsGraph.Add(i, totalDownstreamAog);

                if (totalArrivalOnGreen > linkPivotPair.MaxArrivalOnGreen)
                {
                    linkPivotPair.MaxArrivalOnGreen = totalArrivalOnGreen;
                    linkPivotPair.AogUpstreamPredicted = totalUpstreamAog;
                    linkPivotPair.AogDownstreamPredicted = totalDownstreamAog;
                    linkPivotPair.PaogDownstreamPredicted = Math.Round(totalDownstreamAog / linkPivotPair.TotalVolumeDownstream, 2) * 100;
                    linkPivotPair.PaogUpstreamPredicted = Math.Round(totalUpstreamAog / linkPivotPair.TotalVolumeUpstream, 2) * 100;
                    linkPivotPair.SecondsAdded = i;
                }
            }
            //Get the link totals
            linkPivotPair.AogTotalPredicted = linkPivotPair.MaxArrivalOnGreen;
            linkPivotPair.PaogTotalPredicted = Math.Round(linkPivotPair.AogTotalPredicted / (linkPivotPair.TotalVolumeUpstream + linkPivotPair.TotalVolumeDownstream), 2) * 100;
        }

        private void GetBiasedLinkPivot(int cycleTime, double bias, string biasDirection, List<DateOnly> dates, LinkPivotPair linkPivotPair)
        {
            double upstreamBias = 1;
            double downstreamBias = 1;
            if (biasDirection == "Downstream")
                downstreamBias = 1 + bias / 100;
            else
                upstreamBias = 1 + bias / 100;
            //set the original values to compare against
            linkPivotPair.AogTotalBefore = linkPivotPair.AogDownstreamBefore * downstreamBias +
                             linkPivotPair.AogUpstreamBefore * upstreamBias;
            linkPivotPair.PaogTotalBefore =
                Math.Round(
                    linkPivotPair.AogTotalBefore / (linkPivotPair.TotalVolumeDownstream * downstreamBias + linkPivotPair.TotalVolumeUpstream * upstreamBias), 2) *
                100;
            var maxBiasArrivalOnGreen = linkPivotPair.AogTotalBefore;
            linkPivotPair.MaxArrivalOnGreen = linkPivotPair.AogDownstreamBefore + linkPivotPair.AogUpstreamBefore;


            //add the total to the results grid
            linkPivotPair.ResultsGraph.Add(0, linkPivotPair.MaxArrivalOnGreen);
            linkPivotPair.UpstreamResultsGraph.Add(0, linkPivotPair.AogUpstreamBefore * upstreamBias);
            linkPivotPair.DownstreamResultsGraph.Add(0, linkPivotPair.AogDownstreamBefore * downstreamBias);
            linkPivotPair.AogUpstreamPredicted = linkPivotPair.AogUpstreamBefore;
            linkPivotPair.AogDownstreamPredicted = linkPivotPair.AogDownstreamBefore;
            linkPivotPair.PaogDownstreamPredicted = Math.Round(linkPivotPair.AogDownstreamBefore / linkPivotPair.TotalVolumeDownstream, 2) * 100;
            linkPivotPair.PaogUpstreamPredicted = Math.Round(linkPivotPair.AogUpstreamBefore / linkPivotPair.TotalVolumeUpstream, 2) * 100;
            linkPivotPair.SecondsAdded = 0;

            for (var i = 1; i <= cycleTime; i++)
            {
                double totalBiasArrivalOnGreen = 0;
                double totalArrivalOnGreen = 0;
                double totalUpstreamAog = 0;
                double totalDownstreamAog = 0;

                for (var index = 0; index < dates.Count; index++)
                {
                    locationPhaseService.LinkPivotAddSeconds(linkPivotPair.UpstreamPcd[index], -1);
                    locationPhaseService.LinkPivotAddSeconds(linkPivotPair.DownstreamPcd[index], 1);
                    totalBiasArrivalOnGreen += linkPivotPair.UpstreamPcd[index].TotalArrivalOnGreen * upstreamBias +
                                               linkPivotPair.DownstreamPcd[index].TotalArrivalOnGreen * downstreamBias;
                    totalArrivalOnGreen += linkPivotPair.UpstreamPcd[index].TotalArrivalOnGreen +
                                          linkPivotPair.DownstreamPcd[index].TotalArrivalOnGreen;
                    totalUpstreamAog += linkPivotPair.UpstreamPcd[index].TotalArrivalOnGreen;
                    totalDownstreamAog += linkPivotPair.DownstreamPcd[index].TotalArrivalOnGreen;
                }
                //Add the total aog to the dictionary
                linkPivotPair.ResultsGraph.Add(i, totalBiasArrivalOnGreen);
                linkPivotPair.UpstreamResultsGraph.Add(i, totalUpstreamAog);
                linkPivotPair.DownstreamResultsGraph.Add(i, totalDownstreamAog);

                if (totalBiasArrivalOnGreen > maxBiasArrivalOnGreen)
                {
                    maxBiasArrivalOnGreen = totalBiasArrivalOnGreen;
                    linkPivotPair.MaxArrivalOnGreen = totalArrivalOnGreen;
                    linkPivotPair.AogUpstreamPredicted = totalUpstreamAog;
                    linkPivotPair.AogDownstreamPredicted = totalDownstreamAog;
                    linkPivotPair.PaogDownstreamPredicted = Math.Round(totalDownstreamAog / linkPivotPair.TotalVolumeDownstream, 2) * 100;
                    linkPivotPair.PaogUpstreamPredicted = Math.Round(totalUpstreamAog / linkPivotPair.TotalVolumeUpstream, 2) * 100;
                    linkPivotPair.MaxPercentAog =
                        linkPivotPair.SecondsAdded = i;
                }
            }
            //Get the link totals
            linkPivotPair.AogTotalPredicted = linkPivotPair.MaxArrivalOnGreen;
            linkPivotPair.PaogTotalPredicted = Math.Round(linkPivotPair.AogTotalPredicted / (linkPivotPair.TotalVolumeUpstream + linkPivotPair.TotalVolumeDownstream), 2) * 100;
        }

        private async Task SetPcds(TimeOnly startTime, TimeOnly endTime, List<DateOnly> daysToInclude, int cycleTime, LinkPivotPair linkPivotPair)
        {
            foreach (var dt in daysToInclude)
            {
                var tempStartDate = dt.ToDateTime(startTime);
                var tempEndDate = dt.ToDateTime(endTime);
                var upstreamPcd = await getPcd(cycleTime, linkPivotPair.UpstreamLocationApproach, tempStartDate, tempEndDate);
                linkPivotPair.UpstreamPcd.Add(upstreamPcd);
                linkPivotPair.AogUpstreamBefore += upstreamPcd.TotalArrivalOnGreen;
                linkPivotPair.TotalVolumeUpstream += upstreamPcd.TotalVolume;
                var downstreamPcd = await getPcd(cycleTime, linkPivotPair.DownstreamLocationApproach, tempStartDate, tempEndDate);
                linkPivotPair.DownstreamPcd.Add(downstreamPcd);
                linkPivotPair.AogDownstreamBefore += downstreamPcd.TotalArrivalOnGreen;
                linkPivotPair.TotalVolumeDownstream += downstreamPcd.TotalVolume;
            }
            linkPivotPair.PaogUpstreamBefore = Math.Round(linkPivotPair.AogUpstreamBefore / linkPivotPair.TotalVolumeUpstream, 2) * 100;
            linkPivotPair.PaogDownstreamBefore = Math.Round(linkPivotPair.AogDownstreamBefore / linkPivotPair.TotalVolumeDownstream, 2) * 100;
        }

        private async Task<LocationPhase> getPcd(int cycleTime, Approach approach, DateTime tempStartDate, DateTime tempEndDate)
        {
            var logs = controllerEventLogRepository.GetEventsBetweenDates(approach.Location.LocationIdentifier, tempStartDate.AddHours(-2), tempEndDate.AddHours(2)).ToList();
            var plans = logs.GetPlanEvents(tempStartDate.AddHours(-2), tempEndDate.AddHours(2)).ToList();
            var pcd = await locationPhaseService.GetLocationPhaseDataWithApproach(approach, tempStartDate, tempEndDate, 15, 13, logs, plans, true, null, cycleTime);
            return pcd;
        }
    }
}
