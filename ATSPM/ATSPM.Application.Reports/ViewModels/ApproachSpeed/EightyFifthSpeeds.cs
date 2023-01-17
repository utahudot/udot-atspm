
using System;

namespace ATSPM.Application.Reports.ViewModels.ApproachSpeed
{
    public class EightyFifthSpeeds
    {
        public EightyFifthSpeeds(DateTime startTime, double eightFifthMph)
        {
            StartTime = startTime;
            EightFifthMph = eightFifthMph;
        }

        public DateTime StartTime { get; set; }
        public double EightFifthMph { get; set; }

    }
}