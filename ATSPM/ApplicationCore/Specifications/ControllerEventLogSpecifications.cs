using ATSPM.Data.Models;
using ATSPM.Application.ValueObjects;
using ATSPM.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATSPM.Application.Specifications
{
    public class ControllerLogDateRangeSpecification : BaseSpecification<ControllerLogArchive>
    {
        public ControllerLogDateRangeSpecification(string SignalID, DateTime startTime, DateTime endTime) : base()
        {
            if (string.IsNullOrEmpty(SignalID))
            {
                base.Criteria = c => c.ArchiveDate >= startTime && c.ArchiveDate <= endTime;
            }
            else
            {
                base.Criteria = c => c.SignalID == SignalID && c.ArchiveDate >= startTime && c.ArchiveDate <= endTime;
            }
        }
    }

    public class ControllerLogCodeAndParamSpecification : BaseSpecification<ControllerEventLog>
    {
        public ControllerLogCodeAndParamSpecification(int eventCode) : base()
        {
            base.Criteria = c => c.EventCode == eventCode;

            ApplyOrderBy(o => o.Timestamp);
        }

        public ControllerLogCodeAndParamSpecification(int eventCode, int param) : base()
        {
            base.Criteria = c => c.EventCode == eventCode && c.EventParam == param;

            ApplyOrderBy(o => o.Timestamp);
        }

        public ControllerLogCodeAndParamSpecification(IEnumerable<int> eventCodes) : base()
        {
            base.Criteria = c => eventCodes != null && eventCodes.Contains(c.EventCode);

            ApplyOrderBy(o => o.Timestamp);
        }

        public ControllerLogCodeAndParamSpecification(IEnumerable<int> eventCodes, int param) : base()
        {
            base.Criteria = c => eventCodes != null && eventCodes.Contains(c.EventCode) && c.EventParam == param;

            ApplyOrderBy(o => o.Timestamp);
        }

        public ControllerLogCodeAndParamSpecification(IEnumerable<int> eventCodes, IEnumerable<int> paramCodes) : base()
        {
            base.Criteria = c => eventCodes != null && eventCodes.Contains(c.EventCode) && paramCodes != null && paramCodes.Contains(c.EventParam);

            ApplyOrderBy(o => o.Timestamp);
        }
    }

    public class ControllerLogDateTimeRangeSpecification : BaseSpecification<ControllerEventLog>
    {
        public ControllerLogDateTimeRangeSpecification(int startHour, int startMinute, int endHour, int endMinute) : base()
        {
            base.Criteria = l => l.Timestamp.Hour > startHour && l.Timestamp.Hour < endHour
            || l.Timestamp.Hour == startHour && l.Timestamp.Hour == endHour
            && l.Timestamp.Minute >= startMinute && l.Timestamp.Minute <= endMinute
            || l.Timestamp.Hour == startHour && l.Timestamp.Hour < endHour
            && l.Timestamp.Minute >= startMinute
            || l.Timestamp.Hour < startHour && l.Timestamp.Hour == endHour
            && l.Timestamp.Minute <= endMinute;

            ApplyOrderBy(o => o.Timestamp);
        }
    }
}
