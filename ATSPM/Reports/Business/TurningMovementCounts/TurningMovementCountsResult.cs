using ATSPM.Application.Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.TurningMovementCounts
{
    /// <summary>
    /// Turning Movement Count chart
    /// </summary>
    public class TurningMovementCountsResult
    {
        public TurningMovementCountsResult(
            string chartName,
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
            )
        {
            ChartName = chartName;
            ApproachId = approachId;
            ApproachDescription = approachDescription;
            Start = start;
            End = end;
            Direction = direction;
            Plans = plans;
            Lanes = lanes;
            TotalVolumes = totalVolumes;
            TotalVolume = totalVolume;
            PeakHourVolume = peakHourVolume;
            PeakHourFactor = peakHourFactor;
            LaneUtilizationFactor = laneUtilizationFactor;
        }

        public string ChartName { get; set; }
        public int ApproachId { get; set; }
        public string ApproachDescription { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Direction { get; set; }
        public IReadOnlyList<Plan> Plans { get; set; }
        public IReadOnlyList<Lane> Lanes { get; set; }
        public IReadOnlyList<TotalVolume> TotalVolumes { get; set; }
        public int TotalVolume { get; set; }
        public double? PeakHourVolume { get; set; }
        public double? PeakHourFactor { get; set; }
        public double? LaneUtilizationFactor { get; set; }
    }
}