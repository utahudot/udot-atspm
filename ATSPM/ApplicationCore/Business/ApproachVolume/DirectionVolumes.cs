using System;

namespace ATSPM.Application.Business.ApproachVolume;

public class DirectionVolumes
{
    public DirectionVolumes(DateTime startTime, int volume)
    {
        StartTime = startTime;
        Volume = volume;
    }

    public DateTime StartTime { get; internal set; }
    public int Volume { get; internal set; }

}
