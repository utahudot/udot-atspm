using System;

namespace Reports.Business.Common
{
    public class DataPointBase
    {
        private DateTime _timeStamp;
        public DataPointBase(DateTime timeStamp)
        {
            _timeStamp = DateTime.SpecifyKind(timeStamp, DateTimeKind.Unspecified);
        }
        public DateTime TimeStamp
        {
            get
            {
                return _timeStamp;
            }
            set
            {
                _timeStamp = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
            }
        }
    }
}