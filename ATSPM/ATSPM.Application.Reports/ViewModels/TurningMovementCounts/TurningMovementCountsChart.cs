using ATSPM.Application.Reports.ViewModels;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.ViewModels.TurningMovementCounts
{
    /// <summary>
    /// Turning Movement Count chart
    /// </summary>
    public class TurningMovementCountChart
    {
        public TurningMovementCountChart(
            string chartName,
            string signalId,
            string signalLocation,
            DateTime start,
            DateTime end,
            string direction,
            string movementType,
            string laneType,
            ICollection<Plan> plans,
            ICollection<Lane> lane,
            ICollection<ThruLeft> thruLeft,
            ICollection<ThruRight> thruRight,
            ICollection<TotalVolume> totalVolume)
        {
            ChartName = chartName;
            SignalId = signalId;
            SignalLocation = signalLocation;
            Start = start;
            End = end;
            Direction = direction;
            MovementType = movementType;
            LaneType = laneType;
            Plans = plans;
            Lane = lane;
            ThruLeft = thruLeft;
            ThruRight = thruRight;
            TotalVolume = totalVolume;
        }

        public string ChartName { get; internal set; }
        public string SignalId { get; internal set; }
        public string SignalLocation { get; internal set; }
        public DateTime Start { get; internal set; }
        public DateTime End { get; internal set; }
        public string Direction { get; internal set; }
        public string MovementType { get; internal set; }
        public string LaneType { get; internal set; }
        public System.Collections.Generic.ICollection<Plan> Plans { get; internal set; }
        public System.Collections.Generic.ICollection<Lane> Lane { get; internal set; }
        public System.Collections.Generic.ICollection<ThruLeft> ThruLeft { get; internal set; }
        public System.Collections.Generic.ICollection<ThruRight> ThruRight { get; internal set; }
        public System.Collections.Generic.ICollection<TotalVolume> TotalVolume { get; internal set; }
    }
}