using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.ViewModels.ArrivalOnRed
{
    /// <summary>
    /// Arrival On Red chart
    /// </summary>
    public class ArrivalOnRedResult
    {
        public ArrivalOnRedResult(
            string chartName,
            string signalId,
            string signalLocation,
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
            ICollection<ArrivalsOnRed> arrivalsOnRed)
        {
            ChartName = chartName;
            SignalId = signalId;
            SignalLocation = signalLocation;
            PhaseNumber = phaseNumber;
            PhaseDescription = phaseDescription;
            Start = start;
            End = end;
            TotalDetectorHits = totalDetectorHits;
            TotalArrivalOnRed = totalArrivalOnRed;
            PercentArrivalOnRed = percentArrivalOnRed;
            Plans = plans;
            PercentArrivalsOnRed = percentArrivalsOnRed;
            TotalVehicles = totalVehicles;
            ArrivalsOnRed = arrivalsOnRed;
        }

        public string ChartName { get; set; }
        public string SignalId { get; set; }
        public string SignalLocation { get; set; }
        public int PhaseNumber { get; set; }
        public string PhaseDescription { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public double TotalDetectorHits { get; set; }
        public double TotalArrivalOnRed { get; set; }
        public double PercentArrivalOnRed { get; set; }
        public ICollection<ArrivalOnRedPlan> Plans { get; set; }
        public ICollection<PercentArrivalsOnRed> PercentArrivalsOnRed { get; set; }
        public ICollection<TotalVehicles> TotalVehicles { get; set; }
        public ICollection<ArrivalsOnRed> ArrivalsOnRed { get; set; }

    }
}