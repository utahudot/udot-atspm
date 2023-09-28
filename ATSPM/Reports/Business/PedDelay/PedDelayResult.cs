using Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.PedDelay
{
    /// <summary>
    /// Ped Delay chart
    /// </summary>
    public class PedDelayResult : ApproachResult
    {
        public PedDelayResult(
            string signalId,
            int approachId,
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
            double averageDelay,
            List<PedDelayPlan> plans,
            List<CycleLengths> cycleLengths,
            List<PedestrianDelay> pedestrianDelay,
            List<StartBeginWalk> startOfWalk,
            List<PercentDelayByCycleLength> percentDelayByCycleLength) : base(approachId, signalId, start, end)
        {
            PhaseNumber = phaseNumber;
            PhaseDescription = phaseDescription;
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
        public int PhaseNumber { get; internal set; }
        public string PhaseDescription { get; internal set; }
        public int PedPresses { get; internal set; }
        public double CyclesWithPedRequests { get; internal set; }
        public int TimeBuffered { get; internal set; }
        public int UniquePedestrianDetections { get; internal set; }
        public double MinDelay { get; internal set; }
        public double MaxDelay { get; internal set; }
        public double AverageDelay { get; internal set; }
        public List<PedDelayPlan> Plans { get; internal set; }
        public List<CycleLengths> CycleLengths { get; internal set; }
        public List<PedestrianDelay> PedestrianDelay { get; internal set; }
        public List<StartBeginWalk> StartOfWalk { get; internal set; }
        public List<PercentDelayByCycleLength> PercentDelayByCycleLength { get; internal set; }

    }
}