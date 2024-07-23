using System;

namespace ATSPM.Application.Business.Common
{
    public class Volume
    {
        private readonly int binSizeMultiplier;

        public Volume(DateTime startTime, DateTime endTime, int binSize)
        {
            StartTime = startTime;
            EndTime = endTime;
            if (binSize == 0)
                binSizeMultiplier = 0;
            else
                binSizeMultiplier = 60 / binSize;
        }

        public DateTime StartTime { get; }

        public DateTime EndTime { get; }

        public int DetectorCount { get; set; }

        public int HourlyVolume => DetectorCount * binSizeMultiplier;
    }
}