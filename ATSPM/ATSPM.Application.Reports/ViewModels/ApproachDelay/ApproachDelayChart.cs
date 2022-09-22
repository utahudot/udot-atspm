using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.ViewModels.ApproachDelay
{
    public class ApproachDelayChart
    {
        public ApproachDelayChart(
            string chartName,
            string signalId,
            string signalLocation,
            int phaseNumber,
            string phaseDescription,
            DateTime start,
            DateTime end,
            int totalArrivalOnRed,
            int totalDetectorHits,
            double percentArrivalOnRed,
            List<ApproachDelayPlan> plans,
            List<PercentArrivalsOnReDataPoint> percentArrivalsRed,
            List<TotalVehiclesDataPoint> totalVehicles,
            List<ArrivalsOnRedDataPoint> arrivalsOnRed)
        {
            ChartName = chartName;
            SignalId = signalId;
            SignalLocation = signalLocation;
            PhaseNumber = phaseNumber;
            PhaseDescription = phaseDescription;
            Start = start;
            End = end;
            TotalArrivalOnRed = totalArrivalOnRed;
            TotalDetectorHits = totalDetectorHits;
            PercentArrivalOnRed = percentArrivalOnRed;
            Plans = plans;
            PercentArrivalsRed = percentArrivalsRed;
            TotalVehicles = totalVehicles;
            ArrivalsOnRed = arrivalsOnRed;
        }

        public string ChartName { get; internal set; }
        public string SignalId { get; internal set; }
        public string SignalLocation { get; internal set; }
        public int PhaseNumber { get; internal set; }
        public string PhaseDescription { get; internal set; }
        public DateTime Start { get; internal set; }
        public DateTime End { get; internal set; }
        public int TotalArrivalOnRed { get; internal set; }
        public int TotalDetectorHits { get; internal set; }
        public double PercentArrivalOnRed { get; internal set; }
        public List<ApproachDelayPlan> Plans { get; internal set; }
        public List<PercentArrivalsOnReDataPoint> PercentArrivalsRed { get; internal set; }
        public List<TotalVehiclesDataPoint> TotalVehicles { get; internal set; }
        public List<ArrivalsOnRedDataPoint> ArrivalsOnRed { get; set; }
    }

}
