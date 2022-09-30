using ATSPM.Application.Reports.ViewModels;
using System;

namespace ATSPM.Application.Reports.ViewModels.TurningMovementCounts
{
    /// <summary>
    /// Turning Movement Count chart
    /// </summary>
    public class TurningMovementCountChart
    {
        public string ChartName { get; set; }
        public string SignalId { get; set; }
        public string SignalLocation { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Direction { get; set; }
        public string MovementType { get; set; }
        public string LaneType { get; set; }
        public System.Collections.Generic.ICollection<Plan> Plans { get; set; }
        public System.Collections.Generic.ICollection<Lane> Lane { get; set; }
        public System.Collections.Generic.ICollection<ThruLeft> ThruLeft { get; set; }
        public System.Collections.Generic.ICollection<ThruRight> ThruRight { get; set; }
        public System.Collections.Generic.ICollection<TotalVolume> TotalVolume { get; set; }
    }
}