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
        public ControllerLogDateRangeSpecification(string signalId, DateTime startDate, DateTime endDate) : base()
        {
            base.Criteria = c => c.SignalIdentifier == signalId && c.ArchiveDate.Date >= startDate.Date && c.ArchiveDate.Date <= endDate.Date;

            ApplyOrderBy(o => o.ArchiveDate);
        }
    }

    public class ControllerLogCodeAndParamSpecification : BaseSpecification<ControllerEventLog>
    {
        public ControllerLogCodeAndParamSpecification(int eventCode) : base()
        {
            base.Criteria = c => c.EventCode == eventCode;

            ApplyOrderBy(o => o.TimeStamp);
        }

        public ControllerLogCodeAndParamSpecification(int eventCode, int param) : base()
        {
            base.Criteria = c => c.EventCode == eventCode && c.EventParam == param;

            ApplyOrderBy(o => o.TimeStamp);
        }

        public ControllerLogCodeAndParamSpecification(IEnumerable<int> eventCodes) : base()
        {
            base.Criteria = c => eventCodes != null && eventCodes.Contains(c.EventCode);

            ApplyOrderBy(o => o.TimeStamp);
        }

        public ControllerLogCodeAndParamSpecification(IEnumerable<int> eventCodes, int param) : base()
        {
            base.Criteria = c => eventCodes != null && eventCodes.Contains(c.EventCode) && c.EventParam == param;

            ApplyOrderBy(o => o.TimeStamp);
        }

        public ControllerLogCodeAndParamSpecification(IEnumerable<int> eventCodes, IEnumerable<int> paramCodes) : base()
        {
            base.Criteria = c => eventCodes != null && eventCodes.Contains(c.EventCode) && paramCodes != null && paramCodes.Contains(c.EventParam);

            ApplyOrderBy(o => o.TimeStamp);
        }
    }

    public class ControllerLogDateTimeRangeSpecification : BaseSpecification<ControllerEventLog>
    {
        public ControllerLogDateTimeRangeSpecification(DateTime startDate, DateTime endDate) : base()
        {
            base.Criteria = c => c.TimeStamp >= startDate && c.TimeStamp <= endDate;

            ApplyOrderBy(o => o.TimeStamp);
        }

        public ControllerLogDateTimeRangeSpecification(string signalId, DateTime startDate, DateTime endDate) : base()
        {
            base.Criteria = c => c.SignalIdentifier == signalId && c.TimeStamp >= startDate && c.TimeStamp <= endDate;

            ApplyOrderBy(o => o.TimeStamp);
        }

        public ControllerLogDateTimeRangeSpecification(int startHour, int startMinute, int endHour, int endMinute) : base()
        {
            base.Criteria = l => l.TimeStamp.Hour > startHour && l.TimeStamp.Hour < endHour
            || l.TimeStamp.Hour == startHour && l.TimeStamp.Hour == endHour
            && l.TimeStamp.Minute >= startMinute && l.TimeStamp.Minute <= endMinute
            || l.TimeStamp.Hour == startHour && l.TimeStamp.Hour < endHour
            && l.TimeStamp.Minute >= startMinute
            || l.TimeStamp.Hour < startHour && l.TimeStamp.Hour == endHour
            && l.TimeStamp.Minute <= endMinute;

            ApplyOrderBy(o => o.TimeStamp);
        }
    }
}
