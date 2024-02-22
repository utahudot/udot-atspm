using System;

namespace ATSPM.Application.Business.Common
{
    public class OptionsBase
    {
        private DateTime _start;
        private DateTime _end;
        public DateTime Start
        {
            get
            {
                return _start;
            }
            set
            {
                _start = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
            }
        }
        public DateTime End
        {
            get
            {
                return _end;
            }
            set
            {
                _end = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
            }
        }
        public string locationIdentifier { get; set; }
    }
}