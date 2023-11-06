using System;

namespace ATSPM.ReportApi.Business.Common
{
    public class BaseResult
    {

        public BaseResult(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
