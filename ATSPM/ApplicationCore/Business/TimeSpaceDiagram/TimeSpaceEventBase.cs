using System;

namespace ATSPM.ReportApi.Business.TimeSpaceDiagram
{
    /// <summary>
    /// DTO for storing the initial and final time values that a detector event would follow for creating the time space graph
    /// </summary>
    public class TimeSpaceEventBase
    {
        public TimeSpaceEventBase(DateTime start, DateTime stop, bool? isDetectorOn)
        {
            InitialX = start;
            FinalX = stop;
            IsDetectorOn = isDetectorOn;
        }

        public DateTime InitialX { get; set; }
        public DateTime FinalX { get; set; }
        public bool? IsDetectorOn { get; set; }
    }
}
