using ATSPM.ReportApi.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.ReportApi.Business.ApproachSpeed
{
    public class ApproachSpeedResult : ApproachResult
    {
        public ApproachSpeedResult(
            string signalId,
            int approachId,
            int phaseNumber,
            string phaseDescription,
            DateTime start,
            DateTime end,
            string detectionType,
            int distanceFromStopBar,
            double postedSpeed,
            ICollection<SpeedPlan> plans,
            ICollection<DataPointForInt> averageSpeeds,
            ICollection<DataPointForInt> eightyFifthSpeeds,
            ICollection<DataPointForInt> fifteenthSpeeds) : base(approachId, signalId, start, end)
        {
            PhaseNumber = phaseNumber;
            PhaseDescription = phaseDescription;
            DetectionType = detectionType;
            DistanceFromStopBar = distanceFromStopBar;
            PostedSpeed = postedSpeed;
            Plans = plans;
            AverageSpeeds = averageSpeeds;
            EightyFifthSpeeds = eightyFifthSpeeds;
            FifteenthSpeeds = fifteenthSpeeds;
        }
        public int PhaseNumber { get; set; }
        public string PhaseDescription { get; set; }
        public string DetectionType { get; set; }
        public int DistanceFromStopBar { get; set; }
        public double PostedSpeed { get; set; }
        public ICollection<SpeedPlan> Plans { get; set; }
        public ICollection<DataPointForInt> AverageSpeeds { get; set; }
        public ICollection<DataPointForInt> EightyFifthSpeeds { get; set; }
        public ICollection<DataPointForInt> FifteenthSpeeds { get; set; }
    }
}