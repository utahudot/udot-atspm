using System;

namespace Reports.Business.Common
{
    public class BaseResult
    {

        public BaseResult(DateTime start, DateTime end)
        {
            Start = start;
            End= end;
        }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
