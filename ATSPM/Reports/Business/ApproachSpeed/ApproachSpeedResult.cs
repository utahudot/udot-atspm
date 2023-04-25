using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.ApproachSpeed
{
    public class ApproachSpeedResult
    {
        public ApproachSpeedResult(
            string chartName,
            string signalId,
            string signalLocation,
            int phaseNumber,
            string phaseDescription,
            DateTime start,
            DateTime end,
            string detectionType,
            int distanceFromStopBar,
            double postedSpeed,
            ICollection<SpeedPlan> plans,
            ICollection<AverageSpeeds> averageSpeeds,
            ICollection<EightyFifthSpeeds> eightyFifthSpeeds,
            ICollection<FifteenthSpeeds> fifteenthSpeeds)
        {
            ChartName = chartName;
            SignalId = signalId;
            SignalLocation = signalLocation;
            PhaseNumber = phaseNumber;
            PhaseDescription = phaseDescription;
            Start = start;
            End = end;
            DetectionType = detectionType;
            DistanceFromStopBar = distanceFromStopBar;
            PostedSpeed = postedSpeed;
            Plans = plans;
            AverageSpeeds = averageSpeeds;
            EightyFifthSpeeds = eightyFifthSpeeds;
            FifteenthSpeeds = fifteenthSpeeds;
        }

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
        public ICollection<SpeedPlan> Plans { get; set; }
        public ICollection<AverageSpeeds> AverageSpeeds { get; set; }
        public ICollection<EightyFifthSpeeds> EightyFifthSpeeds { get; set; }
        public ICollection<FifteenthSpeeds> FifteenthSpeeds { get; set; }
    }
}