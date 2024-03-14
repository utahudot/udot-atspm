using ATSPM.Application.Business;
using ATSPM.Application.Business.LinkPivot;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.ReportApi.ReportServices
{
    public class LinkPivotReportService : ReportServiceBase<LinkPivotOptions, IEnumerable<LinkPivotResult>>
    {
        private readonly IApproachRepository approachRepository;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ILocationRepository locationRepository;
        private readonly IRouteRepository routeRepository;
        private readonly IRouteLocationsRepository routeLocationsRepository;

        public LinkPivotReportService(IApproachRepository approachRepository, IControllerEventLogRepository controllerEventLogRepository, ILocationRepository locationRepository, IRouteRepository routeRepository, IRouteLocationsRepository routeLocationsRepository)
        {
            this.approachRepository = approachRepository;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.locationRepository = locationRepository;
            this.routeRepository = routeRepository;
            this.routeLocationsRepository = routeLocationsRepository;
        }
        public override Task<IEnumerable<LinkPivotResult>> ExecuteAsync(LinkPivotOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var routeLocations = GetLocationsFromRouteId(parameter.RouteId);

            List<Location> locations = new List<Location>();
            foreach (var routeSignal in routeLocations)
            {
                var location = locationRepository.GetLatestVersionOfLocation(routeSignal.LocationIdentifier);
                locations.Add(location);
            }

                DateTime startDate = Convert.ToDateTime(lpvm.StartDate.Value.ToShortDateString() + " " + lpvm.StartTime +
                    " " + lpvm.StartAMPM);
                DateTime endDate = Convert.ToDateTime(lpvm.EndDate.Value.ToShortDateString() + " " + lpvm.EndTime +
                    " " + lpvm.EndAMPM);

                LinkPivotServiceReference.LinkPivotServiceClient client =
                    new LinkPivotServiceReference.LinkPivotServiceClient();

                //TestLinkPivot.LinkPivotServiceClient client =
                //    new TestLinkPivot.LinkPivotServiceClient();


                LinkPivotServiceReference.AdjustmentObject[] adjustments;
                //MOEWcfServiceLibrary.AdjustmentObject[] adjustments;
                client.Open();

                //Based on the starting point selected by the user Create the link pivot object
                if (parameter.StartingPoint == "Downstream")
                {
                    adjustments = client.GetLinkPivot(
                        lpvm.SelectedRouteId,
                        startDate,
                        endDate,
                        lpvm.CycleLength,
                        "Downstream",
                        lpvm.Bias,
                        lpvm.BiasUpDownStream,
                        lpvm.PostedDays.DayIDs.Contains("0"),//Sunday,
                        lpvm.PostedDays.DayIDs.Contains("1"),//Monday
                        lpvm.PostedDays.DayIDs.Contains("2"),//Tuesday
                        lpvm.PostedDays.DayIDs.Contains("3"),//Wednesday
                        lpvm.PostedDays.DayIDs.Contains("4"),//Thursday
                        lpvm.PostedDays.DayIDs.Contains("5"),//Friday
                        lpvm.PostedDays.DayIDs.Contains("6"));//Saturday

                }
                else
                {
                    adjustments = client.GetLinkPivot(
                        lpvm.SelectedRouteId,
                        startDate,
                        endDate,
                        lpvm.CycleLength,
                        "Upstream",
                        lpvm.Bias,
                        lpvm.BiasUpDownStream,
                        lpvm.PostedDays.DayIDs.Contains("0"),//Sunday,
                        lpvm.PostedDays.DayIDs.Contains("1"),//Monday
                        lpvm.PostedDays.DayIDs.Contains("2"),//Tuesday
                        lpvm.PostedDays.DayIDs.Contains("3"),//Wednesday
                        lpvm.PostedDays.DayIDs.Contains("4"),//Thursday
                        lpvm.PostedDays.DayIDs.Contains("5"),//Friday
                        lpvm.PostedDays.DayIDs.Contains("6"));//Saturday
                }
                client.Close();
                MOE.Common.Models.ViewModel.LinkPivotResultViewModel lprvm =
                    new MOE.Common.Models.ViewModel.LinkPivotResultViewModel();

                double totalVolume = 0;
                double totalDownstreamVolume = 0;
                double totalUpstreamVolume = 0;
                foreach (LinkPivotServiceReference.AdjustmentObject a in adjustments)
                {
                    lprvm.Adjustments.Add(new MOE.Common.Models.ViewModel.LinkPivotAdjustment(a.LinkNumber, a.SignalId, a.Location,
                        a.Delta, a.Adjustment));

                    lprvm.ApproachLinks.Add(new MOE.Common.Models.ViewModel.LinkPivotApproachLink(a.SignalId,
                        a.Location, a.UpstreamApproachDirection,
                        a.DownSignalId, a.DownstreamLocation, a.DownstreamApproachDirection, a.PAOGUpstreamBefore,
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
                lprvm.ApproachLinks.RemoveAt(lprvm.ApproachLinks.Count - 1);

                //Get the totals
                lprvm.TotalAogDownstreamBefore = adjustments.Sum(a => a.AOGDownstreamBefore);
                lprvm.TotalPaogDownstreamBefore = Math.Round((adjustments.Sum(a => a.AOGDownstreamBefore) / totalDownstreamVolume) * 100);
                lprvm.TotalAogDownstreamPredicted = adjustments.Sum(a => a.AOGDownstreamPredicted);
                lprvm.TotalPaogDownstreamPredicted = Math.Round((adjustments.Sum(a => a.AOGDownstreamPredicted) / totalDownstreamVolume) * 100);

                lprvm.TotalAogUpstreamBefore = adjustments.Sum(a => a.AOGUpstreamBefore);
                lprvm.TotalPaogUpstreamBefore = Math.Round((adjustments.Sum(a => a.AOGUpstreamBefore) / totalUpstreamVolume) * 100);
                lprvm.TotalAogUpstreamPredicted = adjustments.Sum(a => a.AOGUpstreamPredicted);
                lprvm.TotalPaogUpstreamPredicted = Math.Round((adjustments.Sum(a => a.AOGUpstreamPredicted) / totalUpstreamVolume) * 100);

                lprvm.TotalAogBefore = lprvm.TotalAogUpstreamBefore + lprvm.TotalAogDownstreamBefore;
                lprvm.TotalPaogBefore = Math.Round((lprvm.TotalAogBefore / totalVolume) * 100);
                lprvm.TotalAogPredicted = lprvm.TotalAogUpstreamPredicted + lprvm.TotalAogDownstreamPredicted;
                lprvm.TotalPaogPredicted = Math.Round((lprvm.TotalAogPredicted / totalVolume) * 100);

                //once all the data has been set we get the summary info.
                lprvm.SetSummary();

                return PartialView("LinkPivotResult", lprvm);
            return View(lpvm);
        }

        public ActionResult Analysis()
        {
            var lp = new LinkPivotResult();
            lp.Routes = (from a in db.Routes
                         orderby a.RouteName
                         select a).ToList();
            lp.Bias = 0;
            lp.BiasUpDownStream = "Downstream";
            lp.StartingPoint = "Downstream";
            lp.StartDate = DateTime.Today.AddDays(-1);
            lp.EndDate = DateTime.Today.AddDays(-1);
            lp.StartTime = "8:00";
            lp.EndTime = "9:00";
            lp.StartAMPM = "AM";
            lp.EndAMPM = "AM";
            lp.CycleLength = 90;
            return View(lp);
        }

        public List<Location> FillSignals(int routeId)
        {
            var routeLocations = GetLocationsFromRouteId(routeId);

            List<Location> locations = new List<Location>();
            foreach (var routeSignal in routeLocations)
            {
                var location = locationRepository.GetLatestVersionOfLocation(routeSignal.LocationIdentifier);
                locations.Add(location);
            }
            return locations;
        }


        private List<RouteLocation> GetLocationsFromRouteId(int routeId)
        {
            var routeLocations = routeLocationsRepository.GetList().Where(l => l.RouteId == routeId).ToList();
            return routeLocations ?? new List<RouteLocation>();
        }
    }
}
