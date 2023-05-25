using Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.ArrivalOnRed
{
    /// <summary>
    /// Arrival On Red chart
    /// </summary>
    public class ArrivalOnRedResult:ApproachResult
    {
        public ArrivalOnRedResult(
            string signalId,
            int approachId,
            int phaseNumber,
            string phaseDescription,
            DateTime start,
            DateTime end,
            double totalDetectorHits,
            double totalArrivalOnRed,
            double percentArrivalOnRed,
            ICollection<ArrivalOnRedPlan> plans,
            ICollection<PercentArrivalsOnRed> percentArrivalsOnRed,
            ICollection<TotalVehicles> totalVehicles,
            ICollection<ArrivalsOnRed> arrivalsOnRed):base(approachId, signalId,  start, end)
        {
            PhaseNumber = phaseNumber;
            PhaseDescription = phaseDescription;
            TotalDetectorHits = totalDetectorHits;
            TotalArrivalOnRed = totalArrivalOnRed;
            PercentArrivalOnRed = percentArrivalOnRed;
            Plans = plans;
            PercentArrivalsOnRed = percentArrivalsOnRed;
            TotalVehicles = totalVehicles;
            ArrivalsOnRed = arrivalsOnRed;
        }
        public int PhaseNumber { get; set; }
        public string PhaseDescription { get; set; }
        public double TotalDetectorHits { get; set; }
        public double TotalArrivalOnRed { get; set; }
        public double PercentArrivalOnRed { get; set; }
        public ICollection<ArrivalOnRedPlan> Plans { get; set; }
        public ICollection<PercentArrivalsOnRed> PercentArrivalsOnRed { get; set; }
        public ICollection<TotalVehicles> TotalVehicles { get; set; }
        public ICollection<ArrivalsOnRed> ArrivalsOnRed { get; set; }

    }
}