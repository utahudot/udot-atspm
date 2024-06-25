using ATSPM.Application.Business.Common;
using ATSPM.Data.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ATSPM.Application.Business.ArrivalOnRed
{
    public class ArrivalOnRedService
    {

        public ArrivalOnRedService(
            )
        {
        }


        public ArrivalOnRedResult GetChartData(
            ArrivalOnRedOptions options,
            LocationPhase LocationPhase,
            Approach approach
            )
        {
            double totalDetectorHits = 0;
            double totalAoR = 0;
            double totalCars = 0;
            double totalPercentAoR = 0;
            List<DataPointForDouble> percentArrivalsOnReds = new List<DataPointForDouble>();
            List<DataPointForDouble> totalVehicles = new List<DataPointForDouble>();
            List<DataPointForDouble> arrivalsOnReds = new List<DataPointForDouble>();
            if (LocationPhase.Cycles.Count > 0)
            {
                SetDataPoints(options, LocationPhase, ref totalDetectorHits, ref totalAoR, percentArrivalsOnReds, totalVehicles, arrivalsOnReds);
            }
            totalCars = totalDetectorHits;

            if (totalDetectorHits > 0)
                totalPercentAoR = totalAoR / totalCars * 100;

            var plans = GetArrivalOnRedPlans(LocationPhase.Plans);
            return new ArrivalOnRedResult(
                approach.Location.LocationIdentifier,
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

        private static void SetDataPoints(ArrivalOnRedOptions options, LocationPhase LocationPhase, ref double totalDetectorHits, ref double totalAoR, List<DataPointForDouble> percentArrivalsOnReds, List<DataPointForDouble> totalVehicles, List<DataPointForDouble> arrivalsOnReds)
        {

            var dt = LocationPhase.StartDate;
            while (dt < LocationPhase.EndDate)
            {
                double binTotalStops = 0;
                double binPercentAoR = 0;
                double binDetectorHits = 0;
                // Get cycles that start and end within the bin, and the cycle that starts before and ends
                // within the bin, and the cycle that starts within and ends after the bin
                var cycles = LocationPhase.Cycles
                    .Where(c => c.StartTime >= dt && c.StartTime < dt.AddMinutes(options.BinSize)
                || (c.StartTime < dt && c.EndTime >= dt)
                || (c.EndTime >= dt.AddMinutes(options.BinSize)
                && c.StartTime < dt.AddMinutes(options.BinSize)));
                foreach (var cycle in cycles)
                {
                    // Filter cycle events to only include timestamps within the bin
                    var binEvents = cycle.DetectorEvents.Where(e => e.TimeStamp >= dt
                    && e.TimeStamp < dt.AddMinutes(options.BinSize));
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
                percentArrivalsOnReds.Add(new DataPointForDouble(dt, binPercentAoR));
                totalVehicles.Add(new DataPointForDouble(dt, binDetectorHits * (60 / options.BinSize)));
                arrivalsOnReds.Add(new DataPointForDouble(dt, binTotalStops * (60 / options.BinSize)));
                dt = dt.AddMinutes(options.BinSize);
            }
        }

        protected static ReadOnlyCollection<ArrivalOnRedPlan> GetArrivalOnRedPlans(
            List<PurdueCoordinationPlan> planCollection)
        {
            List<ArrivalOnRedPlan> arrivals = new List<ArrivalOnRedPlan>();
            foreach (PurdueCoordinationPlan planPcd in planCollection)
            {
                arrivals.Add(new ArrivalOnRedPlan(
                    planPcd.PlanNumber.ToString(),
                    planPcd.Start,
                    planPcd.End,
                    planPcd.PercentArrivalOnRed,
                    planPcd.PercentRedTime));
            }
            return arrivals.AsReadOnly();

        }


    }
}