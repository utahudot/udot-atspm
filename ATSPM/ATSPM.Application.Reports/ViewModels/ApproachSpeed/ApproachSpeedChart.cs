
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.ViewModels.ApproachSpeed
{
    public class ApproachSpeedChart
    {
        public string ChartName { get; set; }
        public string SignalId { get; set; }
        public string SignalLocation { get; set; }
        public int PhaseNumber { get; set; }
        public string PhaseDescription { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string DetectionType { get; set; }
        public int DistanceFromStopBar { get; set; }
        public double PostedSpeed { get; set; }
        public ICollection<Plans> Plans { get; set; }
        public ICollection<AverageSpeeds> AverageSpeeds { get; set; }
        public ICollection<EightyFifthSpeeds> EightyFifthSpeeds { get; set; }
        public ICollection<FifteenthSpeeds> FifteenthSpeeds { get; set; }
    }
}