#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.ArrivalOnRed/ArrivalOnRedService.cs
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

using System.Collections.ObjectModel;
using Utah.Udot.Atspm.Business.Common;

namespace Utah.Udot.Atspm.Business.ArrivalOnRed
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

        private static void SetDataPoints(
            ArrivalOnRedOptions options,
            LocationPhase LocationPhase,
            ref double totalDetectorHits,
            ref double totalAoR,
            List<DataPointForDouble> percentArrivalsOnReds,
            List<DataPointForDouble> totalVehicles,
            List<DataPointForDouble> arrivalsOnReds)
        {
            var dt = LocationPhase.StartDate;

            while (dt < LocationPhase.EndDate)
            {
                ProcessCycleData(options, LocationPhase, dt, ref totalDetectorHits, ref totalAoR, percentArrivalsOnReds, totalVehicles, arrivalsOnReds);
                dt = dt.AddMinutes(options.BinSize);
            }
        }

        private static void ProcessCycleData(
            ArrivalOnRedOptions options,
            LocationPhase LocationPhase,
            DateTime dt,
            ref double totalDetectorHits,
            ref double totalAoR,
            List<DataPointForDouble> percentArrivalsOnReds,
            List<DataPointForDouble> totalVehicles,
            List<DataPointForDouble> arrivalsOnReds)
        {
            double binTotalStops = 0;
            double binPercentAoR = 0;
            double binDetectorHits = 0;

            var cycles = GetCyclesWithinBin(LocationPhase, dt, options.BinSize);
            foreach (var cycle in cycles)
            {
                ProcessCycleEvents(cycle, dt, options.BinSize, ref binTotalStops, ref binDetectorHits, ref totalAoR, ref totalDetectorHits);
            }

            if (binDetectorHits > 0)
            {
                binPercentAoR = binTotalStops / binDetectorHits * 100;
            }

            AddDataPoints(dt, binPercentAoR, binDetectorHits, binTotalStops, options.BinSize, percentArrivalsOnReds, totalVehicles, arrivalsOnReds);
        }

        private static IEnumerable<CyclePcd> GetCyclesWithinBin(LocationPhase LocationPhase, DateTime dt, double binSize)
        {
            return LocationPhase.Cycles.Where(c =>
                c.StartTime >= dt && c.StartTime < dt.AddMinutes(binSize) ||
                c.StartTime < dt && c.EndTime >= dt ||
                c.EndTime >= dt.AddMinutes(binSize) && c.StartTime < dt.AddMinutes(binSize));
        }

        private static void ProcessCycleEvents(
            CyclePcd cycle,
            DateTime dt,
            double binSize,
            ref double binTotalStops,
            ref double binDetectorHits,
            ref double totalAoR,
            ref double totalDetectorHits)
        {
            var binEvents = cycle.DetectorEvents.Where(e => e.TimeStamp >= dt && e.TimeStamp < dt.AddMinutes(binSize));
            totalDetectorHits += binEvents.Count();
            binDetectorHits += binEvents.Count();

            foreach (var detectorPoint in binEvents)
            {
                if (detectorPoint.YPointSeconds < cycle.GreenLineY)
                {
                    binTotalStops++;
                    totalAoR++;
                }
            }
        }

        private static void AddDataPoints(
            DateTime dt,
            double binPercentAoR,
            double binDetectorHits,
            double binTotalStops,
            double binSize,
            List<DataPointForDouble> percentArrivalsOnReds,
            List<DataPointForDouble> totalVehicles,
            List<DataPointForDouble> arrivalsOnReds)
        {
            percentArrivalsOnReds.Add(new DataPointForDouble(dt, binPercentAoR));
            totalVehicles.Add(new DataPointForDouble(dt, binDetectorHits * (60 / binSize)));
            arrivalsOnReds.Add(new DataPointForDouble(dt, binTotalStops * (60 / binSize)));
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