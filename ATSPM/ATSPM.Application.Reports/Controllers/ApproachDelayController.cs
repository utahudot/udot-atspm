using ATSPM.Application.Reports.ViewModels.ApproachDelay;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApproachDelayController : ControllerBase
    {
        [HttpGet("test")]
        public ApproachDelayChart Test()
        {
            var rnd = new Random();
            var plans = new List<ApproachDelayPlan>();
            for (int i = 0; i < 24; i++)
            {
                plans.Add(new ApproachDelayPlan(
                    rnd.NextDouble(),
                    rnd.NextDouble(),
                    rnd.NextDouble(),
                    new DateTime(2000, 1, 1, i, 0, 0),
                    new DateTime(2000, 1, 1, i, 0, 0).AddHours(1),
                    rnd.Next(100).ToString()));
            }
            var percentArrivalsOnRed = new List<PercentArrivalsOnReDataPoint>();
            for (int i = 0; i < 24; i++)
            {
                percentArrivalsOnRed.Add(new PercentArrivalsOnReDataPoint(
                    new DateTime(2000, 1, 1, i, 0, 0),
                    rnd.NextDouble()));
            }
            var totalVehicles = new List<TotalVehiclesDataPoint>();

            for (int i = 0; i < 24; i++)
            {
                totalVehicles.Add(new TotalVehiclesDataPoint(
                    new DateTime(2000, 1, 1, i, 0, 0),
                    rnd.Next(1000)));
            }
            var arrivalsOnRed = new List<ArrivalsOnRedDataPoint>();
            for (int i = 0; i < 24; i++)
            {
                arrivalsOnRed.Add(new ArrivalsOnRedDataPoint(
                    new DateTime(2000, 1, 1, i, 0, 0),
                    rnd.Next(1000)));
            }
            var approachDelayViewModel = new ApproachDelayChart(
                "Approach Delay Test",
                "9999",
                "Test address",
                99,
                "Test Phase",
                Convert.ToDateTime("1/1/2000 12:00AM"),
                Convert.ToDateTime("1/2/2000 12:00AM"),
                99,
                99,
                .999,
                plans,
                percentArrivalsOnRed,
                totalVehicles,
                arrivalsOnRed

            );
            return approachDelayViewModel;
        }

    }
}
