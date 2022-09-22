using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.ViewModels.ApproachDelay
{
    public class ApproachSpeedChart
    {
        public ApproachSpeedChart(
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
        public string DetectionType { get; internal set; }
        public string DistanceFromStopBar { get; set; }
        public double PostedSpeed { get; set; }
        public List<ApproachSpeedPlan> Plans { get; internal set; }
        public List<AvergeSpeedsDataPoint> AverageSpeeds { get; internal set; }
        public List<EightyFifthSpeedsDataPoint> EightyFifthSpeeds { get; internal set; }
        public List<FifteenthSpeedsDataPoint> FifteenthSpeeds { get; set; }
    }

}
