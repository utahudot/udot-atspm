using ATSPM.Application.Reports.Business.Common;
using Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.TurningMovementCounts
{
    /// <summary>
    /// Turning Movement Count chart
    /// </summary>
    public class TurningMovementCountsResult:ApproachResult
    {
        public TurningMovementCountsResult(
            string signalId,
            int approachId,
            string approachDescription,
            DateTime start,
            DateTime end,
            string direction,
            IReadOnlyList<Plan> plans,
            IReadOnlyList<Lane> lanes,
            IReadOnlyList<TotalVolume> totalVolumes,
            int totalVolume,
            string peakHour,
            double? peakHourVolume,
            double? peakHourFactor,
            double? laneUtilizationFactor
            ):base(approachId, signalId, start, end)
        {
            ApproachId = approachId;
            ApproachDescription = approachDescription;
            Direction = direction;
            Plans = plans;
            Lanes = lanes;
            TotalVolumes = totalVolumes;
            TotalVolume = totalVolume;
            PeakHour = peakHour;
            PeakHourVolume = peakHourVolume;
            PeakHourFactor = peakHourFactor;
            LaneUtilizationFactor = laneUtilizationFactor;
        }

        public string ApproachDescription { get; set; }
        public string Direction { get; set; }
        public IReadOnlyList<Plan> Plans { get; set; }
        public IReadOnlyList<Lane> Lanes { get; set; }
        public IReadOnlyList<TotalVolume> TotalVolumes { get; set; }
        public int TotalVolume { get; set; }
        public string PeakHour { get; }
        public double? PeakHourVolume { get; set; }
        public double? PeakHourFactor { get; set; }
        public double? LaneUtilizationFactor { get; set; }
    }
}