using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Reports.ViewModels.TurningMovementCounts;
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
            ICollection<Plan> plans,
            ICollection<Lane> lanes,
            ICollection<TotalVolume> totalVolumes,
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
        public ICollection<Plan> Plans { get; set; }
        public ICollection<Lane> Lanes { get; set; }
        public ICollection<TotalVolume> TotalVolumes { get; set; }
        public int TotalVolume { get; set; }
        public double? PeakHourVolume { get; set; }
        public double? PeakHourFactor { get; set; }
        public double? LaneUtilizationFactor { get; set; }
    }
}