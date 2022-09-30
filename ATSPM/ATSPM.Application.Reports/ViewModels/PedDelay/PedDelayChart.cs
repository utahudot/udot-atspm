using ATSPM.Application.Reports.ViewModels;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.ViewModels.PedDelay
{
    /// <summary>
    /// Ped Delay chart
    /// </summary>
    public class PedDelayChart
    {
        public PedDelayChart(
            string chartName,
            string signalId,
            string signalLocation,
            int phaseNumber,
            string phaseDescription,
            DateTime start,
            DateTime end,
            int pedPresses,
            double cyclesWithPedRequests,
            int timeBuffered,
            int uniquePedestrianDetections,
            double minDelay,
            double maxDelay,
            string averageDelay,
            ICollection<PedDelayPlan> plans,
            ICollection<CycleLengths> cycleLengths,
            ICollection<PedestrianDelay> pedestrianDelay,
            ICollection<string> startOfWalk,
            ICollection<PercentDelayByCycleLength> percentDelayByCycleLength)
        {
            ChartName = chartName;
            SignalId = signalId;
            SignalLocation = signalLocation;
            PhaseNumber = phaseNumber;
            PhaseDescription = phaseDescription;
            Start = start;
            End = end;
            PedPresses = pedPresses;
            CyclesWithPedRequests = cyclesWithPedRequests;
            TimeBuffered = timeBuffered;
            UniquePedestrianDetections = uniquePedestrianDetections;
            MinDelay = minDelay;
            MaxDelay = maxDelay;
            AverageDelay = averageDelay;
            Plans = plans;
            CycleLengths = cycleLengths;
            PedestrianDelay = pedestrianDelay;
            StartOfWalk = startOfWalk;
            PercentDelayByCycleLength = percentDelayByCycleLength;
        }

        public string ChartName { get; internal set; }
        public string SignalId { get; internal set; }
        public string SignalLocation { get; internal set; }
        public int PhaseNumber { get; internal set; }
        public string PhaseDescription { get; internal set; }
        public DateTime Start { get; internal set; }
        public DateTime End { get; internal set; }
        public int PedPresses { get; internal set; }
        public double CyclesWithPedRequests { get; internal set; }
        public int TimeBuffered { get; internal set; }
        public int UniquePedestrianDetections { get; internal set; }
        public double MinDelay { get; internal set; }
        public double MaxDelay { get; internal set; }
        public string AverageDelay { get; internal set; }
        public ICollection<PedDelayPlan> Plans { get; internal set; }
        public ICollection<CycleLengths> CycleLengths { get; internal set; }
        public ICollection<PedestrianDelay> PedestrianDelay { get; internal set; }
        public ICollection<string> StartOfWalk { get; internal set; }
        public ICollection<PercentDelayByCycleLength> PercentDelayByCycleLength { get; internal set; }

    }
}