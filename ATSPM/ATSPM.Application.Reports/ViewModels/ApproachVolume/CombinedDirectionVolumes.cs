using System;

namespace ATSPM.Application.Reports.ViewModels.ApproachVolume;

public class CombinedDirectionVolumes
{
    public CombinedDirectionVolumes(DateTime startTime, int volume)
    {
        StartTime = startTime;
        Volume = volume;
    }

    public DateTime StartTime { get; internal set; }
    public int Volume { get; internal set; }

}
