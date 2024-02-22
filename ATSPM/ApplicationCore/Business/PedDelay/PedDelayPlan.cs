using ATSPM.ReportApi.Business.Common;
using System;

namespace ATSPM.ReportApi.Business.PedDelay
{
    public class PedDelayPlan : Plan
    {
        public PedDelayPlan(
            string planNumber,
            DateTime startTime,
            DateTime endTime,
            string pedRecallMessage,
            int cyclesWithPedRequests,
            int uniquePedDetections,
            double averageDelaySeconds,
            double averageCycleLengthSeconds) : base(planNumber, startTime, endTime)
        {
            PedRecallMessage = pedRecallMessage;
            CyclesWithPedRequests = cyclesWithPedRequests;
            UniquePedDetections = uniquePedDetections;
            AverageDelaySeconds = averageDelaySeconds;
            AverageCycleLengthSeconds = averageCycleLengthSeconds;
        }

        public string PedRecallMessage { get; internal set; }
        public int CyclesWithPedRequests { get; internal set; }
        public int UniquePedDetections { get; internal set; }
        public double AverageDelaySeconds { get; internal set; }
        public double AverageCycleLengthSeconds { get; internal set; }

    }
}