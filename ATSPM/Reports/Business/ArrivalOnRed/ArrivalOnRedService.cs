using ATSPM.Application.Reports.Business.Common;
using ATSPM.Data.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ATSPM.Application.Reports.Business.ArrivalOnRed
{
    public class ArrivalOnRedService
    {

        public ArrivalOnRedService(
            )
        {
        }


        public ArrivalOnRedResult GetChartData(
            ArrivalOnRedOptions options,
            SignalPhase signalPhase,
            Approach approach
            )
        {
            double totalDetectorHits = 0;
            double totalAoR = 0;
            double totalCars = 0;
            double totalPercentAoR = 0;
            List<PercentArrivalsOnRed> percentArrivalsOnReds = new List<PercentArrivalsOnRed>();
            List<TotalVehicles> totalVehicles = new List<TotalVehicles>();
            List<ArrivalsOnRed> arrivalsOnReds = new List<ArrivalsOnRed>();
            if (signalPhase.Cycles.Count > 0)
            {
                var dt = signalPhase.StartDate;
                while (dt < signalPhase.EndDate)
                {
                    double binTotalStops = 0;
                    double binPercentAoR = 0;
                    double binDetectorHits = 0;
                    // Get cycles that start and end within the bin, and the cycle that starts before and ends
                    // within the bin, and the cycle that starts within and ends after the bin
                    var cycles = signalPhase.Cycles
                        .Where(c => c.StartTime >= dt && c.StartTime < dt.AddMinutes(options.SelectedBinSize))
                        .ToList();
                    //|| c.StartTime < dt && c.EndTime >= dt
                    //|| c.EndTime >= dt.AddMinutes(options.SelectedBinSize)
                    //&& c.StartTime < dt.AddMinutes(options.SelectedBinSize));
                    foreach (var cycle in cycles)
                    {
                        // Filter cycle events to only include timestamps within the bin
                        var binEvents = cycle.DetectorEvents.Where(e => e.TimeStamp >= dt
                        && e.TimeStamp < dt.AddMinutes(options.SelectedBinSize));
                        totalDetectorHits += binEvents.Count();
                        binDetectorHits += binEvents.Count();
                        foreach (var detectorPoint in binEvents)
                            if (detectorPoint.YPointSeconds < cycle.GreenLineY)
                            {
                                binTotalStops++;
                                totalAoR++;
                            }
                        if (binDetectorHits > 0)
                            binPercentAoR = binTotalStops / binDetectorHits * 100;
                    }
                    percentArrivalsOnReds.Add(new PercentArrivalsOnRed(dt, binPercentAoR));
                    totalVehicles.Add(new TotalVehicles(dt, binDetectorHits * (60 / options.SelectedBinSize)));
                    arrivalsOnReds.Add(new ArrivalsOnRed(dt, binTotalStops * (60 / options.SelectedBinSize)));
                    dt = dt.AddMinutes(options.SelectedBinSize);
                }
            }
            totalCars = totalDetectorHits;

            if (totalDetectorHits > 0)
                totalPercentAoR = totalAoR / totalCars * 100;

            var plans = GetArrivalOnRedPlans(signalPhase.Plans, options.ShowPlanStatistics);
            return new ArrivalOnRedResult(
                approach.Signal.SignalIdentifier,
                approach.Id,
                approach.ProtectedPhaseNumber,
                approach.Description,
                options.Start,
                options.End,
                totalDetectorHits,
                totalAoR,
                totalPercentAoR,
                plans,
                percentArrivalsOnReds.AsReadOnly(),
                totalVehicles.AsReadOnly(),
                arrivalsOnReds.AsReadOnly()
                );
        }


        protected ReadOnlyCollection<ArrivalOnRedPlan> GetArrivalOnRedPlans(
            List<PurdueCoordinationPlan> planCollection,
            bool showPlanStatistics)
        {
            List<ArrivalOnRedPlan> arrivals = new List<ArrivalOnRedPlan>();
            foreach (PurdueCoordinationPlan planPcd in planCollection)
            {
                arrivals.Add(new ArrivalOnRedPlan(
                    planPcd.PlanNumber.ToString(),
                    planPcd.StartTime,
                    planPcd.EndTime,
                    planPcd.PercentArrivalOnRed,
                    planPcd.PercentRedTime));
            }
            return arrivals.AsReadOnly();

        }


    }
}