using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using ATSPM.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Specifications
{
    public class IndianaLogLocationFilterSpecification : BaseSpecification<IndianaEvent>
    {
        public IndianaLogLocationFilterSpecification(Location Location) : base()
        {
            base.Criteria = c => c.LocationIdentifier == Location.LocationIdentifier;

            ApplyOrderBy(o => o.Timestamp);
        }
    }

    public class IndianaLogLocationAndParamterFilterSpecification : BaseSpecification<IndianaEvent>
    {
        public IndianaLogLocationAndParamterFilterSpecification(Location Location, int param) : base()
        {
            base.Criteria = c => c.LocationIdentifier == Location.LocationIdentifier && c.EventParam == param;

            ApplyOrderBy(o => o.Timestamp);
        }
    }

    public class IndianaLogDateRangeSpecification : BaseSpecification<CompressedEventLogBase>
    {
        public IndianaLogDateRangeSpecification(string locationId, DateTime startDate, DateTime endDate) : base()
        {
            base.Criteria = c => c.LocationIdentifier == locationId && c.ArchiveDate >= DateOnly.FromDateTime(startDate) && c.ArchiveDate <= DateOnly.FromDateTime(endDate);

            ApplyOrderBy(o => o.ArchiveDate);
        }
    }

    public class IndianaLogCodeAndParamSpecification : BaseSpecification<IndianaEvent>
    {
        public IndianaLogCodeAndParamSpecification(IndianaEnumerations eventCode) : base()
        {
            base.Criteria = c => c.EventCode == eventCode;

            ApplyOrderBy(o => o.Timestamp);
        }

        public IndianaLogCodeAndParamSpecification(IndianaEnumerations eventCode, int param) : base()
        {
            base.Criteria = c => c.EventCode == eventCode && c.EventParam == param;

            ApplyOrderBy(o => o.Timestamp);
        }

        public IndianaLogCodeAndParamSpecification(IEnumerable<IndianaEnumerations> eventCodes) : base()
        {
            base.Criteria = c => eventCodes != null && eventCodes.Contains(c.EventCode);

            ApplyOrderBy(o => o.Timestamp);
        }

        public IndianaLogCodeAndParamSpecification(IEnumerable<IndianaEnumerations> eventCodes, int param) : base()
        {
            base.Criteria = c => eventCodes != null && eventCodes.Contains(c.EventCode) && c.EventParam == param;

            ApplyOrderBy(o => o.Timestamp);
        }

        public IndianaLogCodeAndParamSpecification(IEnumerable<IndianaEnumerations> eventCodes, IEnumerable<int> paramCodes) : base()
        {
            base.Criteria = c => eventCodes != null && eventCodes.Contains(c.EventCode) && paramCodes != null && paramCodes.Contains(c.EventParam);

            ApplyOrderBy(o => o.Timestamp);
        }
    }

    public class IndianaLogDateTimeRangeSpecification : BaseSpecification<IndianaEvent>
    {
        public IndianaLogDateTimeRangeSpecification(DateTime startDate, DateTime endDate) : base()
        {
            base.Criteria = c => c.Timestamp >= startDate && c.Timestamp <= endDate;

            ApplyOrderBy(o => o.Timestamp);
        }

        public IndianaLogDateTimeRangeSpecification(string locationId, DateTime startDate, DateTime endDate) : base()
        {
            base.Criteria = c => c.LocationIdentifier == locationId && c.Timestamp >= startDate && c.Timestamp <= endDate;

            ApplyOrderBy(o => o.Timestamp);
        }

        public IndianaLogDateTimeRangeSpecification(int startHour, int startMinute, int endHour, int endMinute) : base()
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
