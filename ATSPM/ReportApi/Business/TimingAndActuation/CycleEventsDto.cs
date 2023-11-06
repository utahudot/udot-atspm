using System;

namespace ATSPM.ReportApi.Business.TimingAndActuation
{
    public class CycleEventsDto
    {
        public CycleEventsDto(DateTime start, int value)
        {
            Start = start;
            Value = value;
        }

        public DateTime Start { get; set; }
        public int Value { get; set; }

    }
}
