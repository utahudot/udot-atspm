﻿namespace ATSPM.Application.Reports.ViewModels.YellowRedActivations
{
    public class RedClearanceEvents
    {
        public RedClearanceEvents(string startTime, double seconds)
        {
            StartTime = startTime;
            Seconds = seconds;
        }

        public string StartTime { get; internal set; }
        public double Seconds { get; internal set; }
    }
}