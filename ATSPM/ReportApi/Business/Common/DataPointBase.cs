using System;

namespace ATSPM.ReportApi.Business.Common
{
    public class DataPointBase
    {
        private DateTime _timestamp;
        public DataPointBase(DateTime timeStamp)
        {
            _timestamp = DateTime.SpecifyKind(timeStamp, DateTimeKind.Unspecified);
        }
        public DateTime Timestamp
        {
            get
            {
                return _timestamp;
            }
            set
            {
                _timestamp = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
            }
        }
    }
}