using System;

namespace ATSPM.Application.Business.WaitTime
{
    public class Volume
    {
        public Volume(DateTime startTime, int volume)
        {
            StartTime = startTime;
            VolumePerHour = volume;
        }

        public DateTime StartTime { get; internal set; }
        public int VolumePerHour { get; internal set; }
    }
}