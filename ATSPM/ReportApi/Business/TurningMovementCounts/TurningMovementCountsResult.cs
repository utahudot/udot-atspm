using ATSPM.ReportApi.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.ReportApi.Business.TurningMovementCounts
{
    /// <summary>
    /// Turning Movement Count chart
    /// </summary>
    public class TurningMovementCountsResult
    {
        public TurningMovementCountsResult(
            string locationIdentifier,
            string signalDescription,
            DateTime start,
            DateTime end,
            string direction,
            string laneType,
            string movementType,
            IReadOnlyList<Plan> plans,
            IReadOnlyList<Lane> lanes,
            IReadOnlyList<DataPointForInt> totalVolumes,
            int totalVolume,
            string peakHour,
            double? peakHourVolume,
            double? peakHourFactor,
            double? laneUtilizationFactor
            )
        {
            locationIdentifier = locationIdentifier;
            SignalDescription = signalDescription;
            Start = start;
            End = end;
            Direction = direction;
            LaneType = laneType;
            MovementType = movementType;
            Plans = plans;
            Lanes = lanes;
            TotalVolumes = totalVolumes;
            TotalVolume = totalVolume;
            PeakHour = peakHour;
            PeakHourVolume = peakHourVolume;
            PeakHourFactor = peakHourFactor;
            LaneUtilizationFactor = laneUtilizationFactor;
        }

        public string locationIdentifier { get; set; }
        public string SignalDescription { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Direction { get; set; }
        public string LaneType { get; }
        public string MovementType { get; }
        public IReadOnlyList<Plan> Plans { get; set; }
        public IReadOnlyList<Lane> Lanes { get; set; }
        public IReadOnlyList<DataPointForInt> TotalVolumes { get; set; }
        public int TotalVolume { get; set; }
        public string PeakHour { get; }
        public double? PeakHourVolume { get; set; }
        public double? PeakHourFactor { get; set; }
        public double? LaneUtilizationFactor { get; set; }
    }
}