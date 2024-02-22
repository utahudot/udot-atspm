using System;

namespace ATSPM.Application.Business.Common
{
    public class BaseResult
    {
        public BaseResult()
        {
                
        }
        public BaseResult(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
