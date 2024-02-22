using ATSPM.Application.Business.TimingAndActuation;
using System;

namespace ATSPM.Application.Business.TimeSpaceDiagram
{
    public class TimeSpaceDetectorEventDto : DetectorEventBase
    {
        public TimeSpaceDetectorEventDto(
            DateTime start,
            DateTime stop,
            double speedLimit,
            double distanceToStopBar) : base(start, stop)
        {
            SpeedLimit = speedLimit;
            DistanceToStopBar = distanceToStopBar;
        }

        public double SpeedLimit { get; set; }
        public double DistanceToStopBar { get; set; }
    }
}
